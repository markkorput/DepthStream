
import socket, logging
from threading import Thread
from time import sleep
from evento import Event
logger = logging.getLogger(__name__)

#
# Client
#

class ClientThread:
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
  
  def start(self):
    def threadFunc():
      while self.running:
        # create socket
        with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
          # non-blocking
          # s.setblocking(0)

          # connect to server
          errno = s.connect_ex((self.host, self.port))

          if errno != 0:
            logger.warning('Failed to connect to {}:{}'.format(self.host, self.port))
            self.connectFailureEvent((self.host, self.port))
          else:
            self.activeSocket = s
            self.connectEvent(s)

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

  def sendall(self, data):
    if not self.activeSocket:
      logger.warn('No active socket, can\'t send data: {}'.format(data))
      return None
    return self.activeSocket.sendall(data)

class PacketStreamReceiver:
  def __init__(self, handler, numBuffers=2):
    self.packetHandler = handler
    self.stoppedByOwner = False    
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

  def receive(self, socket):
    # this func will be pass on to the receive_bytes classmethod
    def continueFunc():
      return not self.stoppedByOwner

    # loop until told to stop
    while not self.stoppedByOwner:
      # select next buffer
      buffer = self.buffers[self.bufferCursor]

      # logger.info('Reading header...')
      # read packet header (body size)
      buffer, size = PacketStreamReceiver.receive_bytes(socket, 4, buffer, continueFunc)

      if not buffer:
        logger.debug('Connection terminated while reading header')
        return

      body_size = PacketStreamReceiver.parse_int(buffer)
      logger.debug('Got body size: {}'.format(body_size))

      buffer, size = PacketStreamReceiver.receive_bytes(socket, body_size, buffer, continueFunc)

      if not buffer:
        logger.debug('Connection terminated while reading body')
        return

      self.packetHandler(buffer, size)

      self.buffers[self.bufferCursor] = buffer # receive_bytes method might have allocated a newer bigger buffer
      self.bufferCursor = self.bufferCursor+1 if self.bufferCursor+1 < len(self.buffers) else 0

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

class ServerThread:
  def __init__(self, port=4445, start=True, connectionHandler=None, maxConnections=1, maxPortAttempts=5, socketTimeout=0.5, idleFunc=None):
    self.port = port
    self.connectionHandler = connectionHandler
    self.maxConnections = maxConnections
    self.maxPortAttempts = maxPortAttempts
    self.socketTimeout = socketTimeout
    self.idleFunc = idleFunc

    self.threadHandle = None
    self.running = False

    if start:
      self.start()

  def start(self):
    def threadFunc():
      s = None
      while self.running:
        # make sure we have a valid socket
        if not s:
          s = ServerThread.createSocket(self.port, self.maxConnections, maxPortAttempts=self.maxPortAttempts, socketTimeout=self.socketTimeout)

        # failed to create socket, wait and retry?
        if not s:
          logger.warning('Could not create service socket on port: {}'.format(self.port))
          sleep(0.5)
          continue

        # accept new connection on socket
        clientsocket = None
        addr = None

        try:
          clientsocket, addr = s.accept()
        except socket.timeout:
          clientsocket = None
        except BlockingIOError as err:
          clientsocket = None
          logging.warn('Accept error: {}'.format(err.errno))
          sleep(0.5)

        if clientsocket and addr and self.connectionHandler:
          self.connectionHandler(clientsocket, addr)

        if self.idleFunc:
          self.idleFunc()

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


class PacketConsumer:
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

    body_size = PacketStreamReceiver.parse_int(data)
    logger.debug('consumer got package size: {}'.format(body_size))

    data = self.socket.recv(body_size)
    logger.debug('consumer received packet data ({} bytes): {}'.format(len(data), data))

    return data

from json import dumps as json_dumps
class PacketService:
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
    # polls all connected consumer sockets for
    # incoming data and responds if necessary
    self.recv()

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

  def onConnection(self, socket, addr):
    self.consumers.append(PacketConsumer(socket, addr))

  def onDisconnect(self, consumer):
    self.consumers.remove(consumer)
    logger.debug('Client disconnected, {} connected consumers left'.format(len(self.consumers)))

  def recv(self):
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

