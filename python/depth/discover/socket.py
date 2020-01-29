
import socket, logging
from threading import Thread
from time import sleep
from evento import Event

logger = logging.getLogger(__name__)

#
# Client
#

class SocketClientThread:
  def __init__(self, host='127.0.0.1', port=4445, start=True, maxBytes=1024, connectionFunc=None):
    self.port = port
    self.host = host
    self.running = True
    self.maxBytes = maxBytes
    self.delay = 0.8
    self.connectionFunc = connectionFunc
    self.threadHandle = None

    self.connectEvent = Event()
    self.disconnectEvent = Event()
    self.connectFailureEvent = Event()
    self.dataEvent = Event()

    if start:
      self.start()
  
  def start(self):
    def threadFunc():
      while self.running:
        # create socket
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
          # connect
          errno = s.connect_ex((self.host, self.port))

          if errno != 0:
            logger.warning('Failed to connect to {}:{}'.format(self.host, self.port))
            self.connectFailureEvent((self.host, self.port))
          else:
            self.connectEvent(s)
            if self.connectionFunc:
              self.connectionFunc(s)
            else:
              # receive
              while self.running:    
                data = s.recv(self.maxBytes)

                if not data:
                  self.disconnectEvent(s)
                  break

                self.dataEvent(data)

        sleep(self.delay)
    self.threadHandle = Thread(target=threadFunc)
    self.threadHandle.start()

  def stop(self, wait=True):
    self.running = False

    if self.threadHandle and wait:
      self.threadHandle.join()

    self.threadHandle = None

class PacketStreamReceiver:
  def __init__(self, handler):
    self.stoppedByOwner = False
    self.packetHandler = handler
    self.buffer = None
    self.lastPacketSize = None

  @classmethod
  def receive_bytes(cls, s, size, buffer=None, continueFunc=None):
    if not buffer or len(buffer) < size:
      buffer = bytearray(size)

    view = memoryview(buffer)
    bytesleft = size

    while continueFunc != None or continueFunc():
      if bytesleft == 0:
        # logger.debug('Read all {} bytes'.format(size))
        return (buffer, size)

      nbytes = s.recv_into(view, bytesleft)

      if not nbytes:
        logger.debug('Connection terminated while reading {} bytes (received: {})'.format(size, size-bytesleft))
        return None, 0

      view = view[nbytes:]
      bytesleft -= nbytes

    logger.debug('receive_bytes discontinued ({}/{})'.format(size-bytesleft, size))
    return None, 0

  @classmethod
  def parse_int(cls, buf):
    b0 = 0xFF & buf[0]
    b1 = 0xFF & buf[1]
    b2 = 0xFF & buf[2]
    b3 = 0xFF & buf[3]
    return ((b0 << 24) | (b1 << 16) | (b2 << 8) | b3)

  def receive(self, socket):

    def continueFunc():
      return not self.stoppedByUser

    # loop until told to stop
    while not self.stoppedByOwner:
      # logger.info('Reading header...')
      # read packet header (body size)
      self.buffer, size = PacketStreamReceiver.receive_bytes(socket, 4, self.buffer, continueFunc)

      if not self.buffer:
        logger.debug('Connection terminated while reading header')
        return

      body_size = PacketStreamReceiver.parse_int(self.buffer)
      logger.debug('Got body size: {}'.format(body_size))

      self.buffer, size = PacketStreamReceiver.receive_bytes(socket, body_size, self.buffer, continueFunc)

      if not self.buffer:
        logger.debug('Connection terminated while reading body')
        return

      self.packetHandler(self.buffer, size)
      self.lastPacketSize = size

  def stop(self):
    self.stoppedByOwner = True

#
# Service
#

class ServerThread:
  def __init__(self, port=4445, start=True, connectionHandler=None, maxConnections=1):
    self.port = port
    self.connectionHandler = connectionHandler
    self.maxConnections = maxConnections

    self.threadHandle = None
    self.running = False

    if start:
      self.start()

  def start(self):
    def threadFunc():
      s = None
      while self.running:
        if not s:
          s = ServerThread.createSocket(self.port, self.maxConnections)

        if not s:
          logger.warning('Could not create service socket on port: {}'.format(self.port))
          sleep(0.5)
          continue

        clientsocket = None
        addr = None

        try:
          clientsocket, addr = s.accept()
        except BlockingIOError as err:
          logging.warn('Accept error: {}'.format(err.errno))
          sleep(0.5)

        if clientsocket and addr and self.connectionHandler:
          self.connectionHandler(clientsocket, addr)

        # with con
        #   if not data:
        #     #  self.connectionEvent(conn, addr)

        #     print('Connected by', addr)
        #     self.connectionEvent(conn, addr)
        #     # while True:
        #     #     data = conn.recv(1024)
        #     #     if not data:
        #     #         break
        #         # conn.sendall(data)
      if s:
        s.close()


    self.running = True
    self.threadHandle = Thread(target=threadFunc)
    self.threadHandle.start()

  def stop(self, wait=True):
    self.running = False

    if self.threadHandle and wait:
      self.threadHandle.join()

    self.threadHandle = None

  @classmethod
  def createSocket(cls, portNum, maxConnections):   
    sock = None

    try:
      sock = socket.socket(
        socket.AF_INET, #Internet
        # socket.SOCK_DGRAM #UDP
        socket.SOCK_STREAM)
    except OSError as msg:
      return None

    try:
      #Bind to any available address on port *portNum*
      sock.bind(('127.0.0.1',portNum))
      
      sock.listen(maxConnections)

      #Prevent the socket from blocking until it receives all the data it wants
      #Note: Instead of blocking, it will throw a socket.error exception if it
      #doesn't get any data
      # sock.setblocking(0)
    except OSError as msg:
      sock.close()
      return None

    return sock

class PacketService:
  def __init__(self, serviceId, port=4445, start=True):
    self.serviceId = serviceId # will be used (WIP) for announcing present of service via broadcasts
    self.port = port
    self.clients = []

    if start:
      self.start()

  def start(self):
    self.serverThread = ServerThread(self.port, connectionHandler=self.onConnection)

  def stop(self):
    if self.serverThread:
      self.serverThread.stop()
      self.serverThread = None

  def submit(self, buffer, size):
    logger.debug('PacketService.submit, to {} clients'.format(len(self.clients)))
    # to do; write data to socket
    clients = self.clients.copy()

    header = bytearray(4)
    header[0] = (size >> 24) & 0xFF
    header[1] = (size >> 16) & 0xFF
    header[2] = (size >> 8) & 0xFF
    header[3] = (size >> 0) & 0xFF

    body = memoryview(buffer)[0:size]

    for client in clients:
      socket, addr = client
      socket.sendall(header)
      socket.sendall(body)

  def onConnection(self, socket, addr):
    self.clients.append((socket,addr))


