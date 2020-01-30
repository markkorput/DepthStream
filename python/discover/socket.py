
import socket, logging
from threading import Thread
from time import sleep
from evento import Event
logger = logging.getLogger(__name__)

#
# Client
#

def createClientSocket(host='127.0.0.1', port=4445, socketTimeout=0.1):
  logger.info('createClientSocket: {}:{}'.format(host,port))
  s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
  
  # non-blocking
  if socketTimeout != None:
    s.settimeout(socketTimeout)

  # connect to server
  errno = s.connect_ex((host, port))

  if errno:
    logger.warn('failed to connect to server: {}'.format(errno))
    return None
  
  return s


class ClientThread:
  """
  ClientThread manages a thread that creates a socket to connect to
  a server. Once the connection is established, control is passed on to
  the connectionFunc specified in the constructor. When the connectionFunc
  returns, this is considered to be an end of the connection and the thread
  will try to re-establish connection with the server.
  """

  def __init__(self, host='127.0.0.1', port=4445, reconnectDelay=1.0, connectionFunc=None, maxBytes=1024, start=True):
    self.host = host
    self.port = port

    self.reconnectDelay = 0.8
    self.connectionFunc = connectionFunc
    self.maxBytes = maxBytes
    self.threadHandle = None

    self.connectEvent = Event()
    self.disconnectEvent = Event()
    self.connectFailureEvent = Event()
    self.dataEvent = Event()

    self.activeSocket = None
    self.activeConnectionFunc = None
    self.running = False

    if start:
      self.running = True
      self.start()
  
  def __del__(self):
    self.stop()

  def start(self):
    def threadFunc():
      while self.running:
        if not self.activeSocket:
          s = createClientSocket(self.host, self.port, socketTimeout=None)

          if not s:
            logger.warning('Failed to connect to {}:{}'.format(self.host, self.port))
            self.connectFailureEvent((self.host, self.port))
            sleep(0.5)
            continue

          self.activeSocket = s
          self.connectEvent(s)

        s = self.activeSocket

        # not that we're connected, pass control to the connectionFunc specified in our
        # constructor, or, if no connectionFunc was specifed, run our default connection
        # func which published incoming data usng self.dataEvent
        func = self.connectionFunc if self.connectionFunc else self._defaultConnectionFunc

        # connection func can be a list, if it's not, for simplicity
        # we'll just turn it into a single item list here
        if type(func) != type([]) and type(func) != type(()):
          func = [func]

        for f in func:
          self.activeConnectionFunc = f
          f(s)

        self.activeConnectionFunc = None
        self.activeSocket = None
        self.disconnectEvent(s)

        sleep(self.reconnectDelay)

    self.threadHandle = Thread(target=threadFunc)
    self.threadHandle.start()

  def stop(self, wait=True, tryToStopConnectionFunc=True):
    logger.debug('ClientThread.stop')
    self.running = False

    # BONUS; if we've passed on the control to an external
    # we'll try to see if it has a stop method, and execute
    # it if it does.
    if tryToStopConnectionFunc and self.activeConnectionFunc:
      if 'stop' in dir(self.activeConnectionFunc):
        logger.info('ClientThread found stop method on activeConnectionFunc, attempting to stop it...')
        self.activeConnectionFunc.stop()
        self.activeConnectionFunc = None

    if self.threadHandle and wait:
      self.threadHandle.join()

    self.threadHandle = None

  def _defaultConnectionFunc(self, socket):
    while self.running:    
      data = socket.recv(self.maxBytes)

      if not data:
        return

      self.dataEvent(data)

  def getSocket(self):
    return self.activeSocket

class PacketReader:
  """
  Reads packets (header/body) from a socket.
  """

  def __init__(self, numBuffers=2):
    self.buffers = list(map(lambda i: None, range(max(1,numBuffers))))
    self.bufferCursor = 0

  @classmethod
  def receive_bytes(cls, s, size, buffer=None, continueFunc=None, allowGrowBuffer=False):
    # if no buffer specified, we'll just create or own
    if not buffer:
      buffer = bytearray(size)

    # if buffer is too small to hold specified number of bytes
    if len(buffer) < size:
      # either grow it...
      if allowGrowBuffer:
        # grow buffer
        buffer.extend(bytearray(size - len(buffer)))
      # ... or abort
      else:
        # logger.warning('PacketStreamReceiver receive_bytes aborted because buffer was too small ({}) for specified number of bytes to fetch ({})'.format(len(buffer), size))
        # return None, 0

        # simply create new buffer
        buffer = bytearray(size)

    view = memoryview(buffer)
    bytesleft = size

    while continueFunc == None or continueFunc():
      if bytesleft == 0:
        logger.debug('Read all {} bytes'.format(size))
        return buffer, size

      nbytes = s.recv_into(view, bytesleft)

      if not nbytes:
        logger.debug('Connection terminated while reading {} bytes (received: {})'.format(size, size-bytesleft))
        return None, 0

      # logger.debug('Received {} bytes'.format(nbytes))
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

  def read(self, socket, onDisconnect=None, continueFunc=None):
    if socket == None:
      if onDisconnect: onDisconnect()
      return None
    # logger.debug('PacketReader.read')

    # select next buffer
    buffer = self.buffers[self.bufferCursor]

    # logger.info('Reading header...')
    # read packet header (body size)
    buffer, size = PacketReader.receive_bytes(socket, 4, buffer, continueFunc)

    if not buffer:
      logger.debug('Connection terminated while reading header')
      if onDisconnect: onDisconnect()
      return None

    body_size = PacketReader.parse_int(buffer)
    logger.debug('Got body size: {}'.format(body_size))

    buffer, size = PacketReader.receive_bytes(socket, body_size, buffer, continueFunc)

    if not buffer:
      logger.debug('Connection terminated while reading body')
      if onDisconnect: onDisconnect()
      return None

    self.buffers[self.bufferCursor] = buffer # receive_bytes method might have allocated a newer bigger buffer
    self.bufferCursor = self.bufferCursor+1 if self.bufferCursor+1 < len(self.buffers) else 0

    return buffer, size

class PacketStreamReceiver:
  """
  Receives packets (header/body pairs) in and endless loop, passing received
  packets to the handler specified in the constructor. Designed to be invoked
  using the receive method inside a ClientThread.
  """

  def __init__(self, handler, numBuffers=2):
    self.reader = PacketReader(numBuffers=numBuffers)
    self.packetHandler = handler
    self.stoppedByOwner = False    

  def receive(self, socket):
    self.disconnected = False

    # loop until told to stop
    while not self.stoppedByOwner and not self.disconnected:
      def onDisconnect():
        self.disconnected = True

      packet = self.reader.update(onDisconnect=onDisconnect)

      if packet:
        buffer, size = packet
        self.packetHandler(buffer, size)

  def stop(self):
    logger.debug('PacketStreamReceiver.stop')
    self.stoppedByOwner = True

  __call__ = receive

def readPacketsIntoBuffer(buffer):
  """
  This function returns an instance of PacketStreamReceiver,
  which is callable with a socket param, and can thuse be
  used as 'connectionFunc' option for a SocketClientThread.
  """
  return PacketStreamReceiver(handler=buffer.write_buffer_and_size)

def appendPacketsIntoList(queue):
  def handler(data, size):
    queue.append((data,size))
  
  return PacketStreamReceiver(handler=handler)

from json import loads as json_loads
from json.decoder import JSONDecodeError

class PacketStreamInfo:
  """
  PacketStreamInfo wraps all logic for sending outgoing,
  and processing incoming packet stream information.
  """

  REQUEST_BODY = b'GET/info.json'

  def __init__(self, data):
    self.info = None

  @classmethod
  def sendRequest(cls, socket):
    # we're sorta faking restfull HTTP interface
    request = PacketStreamInfo.REQUEST_BODY
    return PacketService.sendDataAsPacket(socket, request)

  @classmethod
  def parse(cls, data, size):
    if size > 2048:
      return None

    if len(data) != size:
      data = data[0:size] #memoryview(data)[0:size].bytes

    json = None

    try:
      json = json_loads(data.decode('utf-8'))
    except JSONDecodeError as err:
      return None
    except UnicodeDecodeError as err:
      return None

    # check if there's a valid 'format' attribute in the json
    format = json['format'] if 'format' in json else ''

    if format.startswith('streaminfo-'):
      return json
    
    return None

#
# Service
#

class SocketConnectionAccepter:
  """
  Creates and manages a socket on which incoming connections are accepted.
  The update() method should be called in the application's main loop,
  to process new incoming connections.
  """

  def __init__(self, port=4445, maxConnections=5, maxPortAttempts=5, socketTimeout=0.2, start=True):
    self.port = port
    self.maxConnections = maxConnections
    self.maxPortAttempts = maxPortAttempts
    self.socketTimeout = socketTimeout

    self.activeSocket = None

    if start:
      self.start()

  def __del__(self):
    self.stop()

  def start(self):
    s = SocketConnectionAccepter.createSocket(self.port, maxConnections=self.maxConnections, maxPortAttempts=self.maxPortAttempts, socketTimeout=self.socketTimeout)

    # failed to create socket, wait and retry?
    if not s:
      logger.warning('Could not create service socket on port: {}'.format(self.port))
      return

    self.activeSocket = s

  def stop(self):
    if self.activeSocket:
      self.activeSocket.close()
      self.activeSocket = None

  def update(self, onConnection=None):
    s = self.activeSocket

    # make sure we have a valid socket
    if not s:
      self.start()

    clientsocket = None
    addr = None

    # accept new connection on socket (non-blocking)
    try:
      clientsocket, addr = s.accept()
    except socket.timeout:
      clientsocket = None
      return
    except BlockingIOError as err:
      clientsocket = None
      logging.warn('Accept error: {}'.format(err.errno))

    # notify caller about any new connections
    if clientsocket and addr:
      if onConnection:
        onConnection(clientsocket, addr)
      else:
        logger.warn('Unhandled new socket connection')
        clientsocket.close()

  @classmethod
  def createSocket(cls, portNum, maxConnections, maxPortAttempts=1, socketTimeout=None):
    sock = None

    try:
      sock = socket.socket(
        socket.AF_INET, #Internet
        # socket.SOCK_DGRAM #UDP
        socket.SOCK_STREAM)
    except OSError as msg:
      return None

    attempt = 0
    while attempt < maxPortAttempts:
      try:
        host = ''
        port = portNum + attempt
        logger.info('Trying to bind server socket to: {}:{}...'.format(host, port))

        #Bind to any available address on port *portNum*
        sock.bind((host, port))
      
        sock.listen(maxConnections)

        #Prevent the socket from blocking until it receives all the data it wants
        #Note: Instead of blocking, it will throw a socket.error exception if it
        #doesn't get any data
        # sock.setblocking(0)
        if socketTimeout:
          sock.settimeout(socketTimeout)

      except OSError as msg:
        # sock.close()
        logging.warn('binding socket failed')
        attempt += 1
        continue

      logging.warn('socket bound to {}'.format(portNum+attempt))
      return sock

    logger.warn("Failed to bind server thread socket after {} port attempt(s)".format(maxPortAttempts))
    return None

class ServerThread:
  """
  Manages a thread that runs a socket connection accepter
  """

  def __init__(self, port=4445, start=True, connectionHandler=None, maxConnections=1, maxPortAttempts=5, socketTimeout=0.5, idleFunc=None):
    self.connectionAccepter = SocketConnectionAccepter(port=port, maxConnections=maxConnections, maxPortAttempts=maxPortAttempts, socketTimeout=socketTimeout, start=start)
    self.idleFunc = idleFunc

    self.connectionHandler = connectionHandler
    self.threadHandle = None
    self.running = False

    if start:
      self.start()

  def start(self):
    def threadFunc():
      self.connectionAccepter.start()

      while self.running:
        self.connectionAccepter.update(onConnection=self.handleConnection)
        sleep(0.2)

      self.connectionAccepted.stop()

    self.running = True
    self.threadHandle = Thread(target=threadFunc)
    self.threadHandle.start()

  def stop(self, wait=True):
    self.running = False

    if self.threadHandle and wait:
      self.threadHandle.join()

    self.threadHandle = None

  def handleConnection(self, socket, addr):
    if self.connectionHandler:
      self.connectionHandler(socket, addr)
      return

    logger.warn("ServerThread has not connection handler")   

class PacketConsumer:
  """
  PacketConsumer represents a single consumer
  connected via sockets. It provides an interface
  to receive data (non-blocking) from the specified consumer.
  """
  def __init__(self, socket, addr, timeout=0.2):
    self.socket = socket
    self.addr = addr

    self.socket.settimeout(timeout)

  def __del__(self):
    self.close()

  def close(self):
    self.socket.close()

  def recv(self):
    # logger.info('Reading header...')
    # read packet header (body size)
    try:
      data = self.socket.recv(4)
    except socket.timeout:
      return None
    except ConnectionResetError:
      return False

    if len(data) != 4:
      return None

    body_size = PacketReader.parse_int(data)
    logger.debug('consumer got package size: {}'.format(body_size))

    data = self.socket.recv(body_size)
    logger.debug('consumer received packet data ({} bytes): {}'.format(len(data), data))

    return data

from json import dumps as json_dumps


class PacketService:
  """
  PacketService Manages a list of connected consumers.
  and sends packets (header/body) of data to them
  """  

  DEFAULT_INFO = {'format':'streaminfo-0.0.1'}

  def __init__(self, serviceId, port=4445, start=True, infoData=DEFAULT_INFO):
    self.serviceId = serviceId # will be used (WIP) for announcing present of service via broadcasts
    self.port = port
    self.infoData = infoData
    self.consumers = []

    if not 'format' in self.infoData:
      data = {}
      data.update(PacketService.DEFAULT_INFO)
      data.update(self.infoData)
      self.infoData = data

    if start:
      self.start()

  def __del__(self):
    self.stop()

  def start(self):
    pass

  def stop(self):
    self.consumers.clear()

  def update(self):
    self.processIncomingConsumerData()

  def submit(self, buffer, size):
    logger.debug('PacketService.submit, to {} consumers'.format(len(self.consumers)))

    body = memoryview(buffer)[0:size]

    failures = []

    consumers = self.consumers.copy()

    for c in consumers:
      if not PacketService.sendDataAsPacket(c.socket, body, size=size):
        failures.append(c)
    
    for f in failures:
      self.onDisconnect(c)

  @classmethod
  def sendDataAsPacket(cls, socket, data, size=None):
    if size == None:
      size = len(data)
    
    if size != len(data):
      data = memoryview(data)[0:size]

    # create 4-byte unsigned int header with body size
    header = bytearray(4)
    header[0] = (size >> 24) & 0xFF
    header[1] = (size >> 16) & 0xFF
    header[2] = (size >> 8) & 0xFF
    header[3] = (size >> 0) & 0xFF

    try:
      socket.sendall(header)
      socket.sendall(data)
    except OSError:
      return False

    return True

  def addConsumerSocket(self, socket, addr):
    self.consumers.append(PacketConsumer(socket, addr))

  def onDisconnect(self, consumer):
    self.consumers.remove(consumer)
    logger.debug('Client disconnected, {} connected consumers left'.format(len(self.consumers)))

  def processIncomingConsumerData(self):
    """
    Polls all connected consumer sockets for
    incoming data and responds if necessary
    """
    cs = self.consumers.copy()
    for c in cs:
      data = c.recv()

      if data == False:
        self.onDisconnect(c)
        continue

      if data == PacketStreamInfo.REQUEST_BODY:
        info = self.infoData
        response = json_dumps(info).encode('utf-8')

        if PacketService.sendDataAsPacket(c.socket, response):
          logger.info('Sent stream info request response to consumer ({} bytes): {}'.format(len(response), response))
        else:
          logger.warn('Failed to send stream info')
