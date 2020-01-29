
import socket, logging
from threading import Thread
from time import sleep
from evento import Event

logger = logging.getLogger(__name__)

class SocketClientThread:
  def __init__(self, host='127.0.0.1', port=4445, start=True, maxBytes=1024, connectionFunc=None):
    self.port = port
    self.host = host
    self.running = True
    self.maxBytes = maxBytes
    self.delay = 0.8
    self.connectionFunc = connectionFunc

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

  def stop(self):
    self.running = False

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




# def createSocket(portNum):
#   logger.debug('createSocket({})'.format(portNum))

#   socket = socket.socket(
#     socket.AF_INET, #Internet
#     # socket.SOCK_DGRAM #UDP
#     socket.SOCK_STREAM)
                            
#   #Bind to any available address on port *portNum*
#   socket.bind(("",portNum))
  
#   #Prevent the socket from blocking until it receives all the data it wants
#   #Note: Instead of blocking, it will throw a socket.error exception if it
#   #doesn't get any data
  
#   socket.setblocking(0)
  
#   # print "RX: Receiving data on UDP port " + str(portNum)
#   # print ""
#   return socket


# def recv(socket, maxBytes=1024):
#   logger.debug('recv(socket={}, maxBytes={})'.format(socket, maxBytes))

#   try:
#       #Attempt to receive up to 1024 bytes of data
#       data,addr = socket.recvfrom(maxBytes) 
#       #Echo the data back to the sender
#       # socket.sendto(str(data),addr)
#       # print('received {} from {}'.format(data, addr)) # => received b'/abc\x00\x00\x00\x00,\x00\x00\x00' from ('127.0.0.1', 63257)
#       # if dataFunc:
#         # dataFunc(data, addr)
#       return (data, addr)

#   except socket.error:
#       #If no data is received, you get here, but it's not an error
#       #Ignore and continue
#       pass
  
#   return None


# class Transmitter:
#   def __init__(self, port=4445, host=''):
#     self.host = host
#     self.port = port
#     self.exit = False

#     self.threadHandle = None
#     self.connected = False

#   def start(self):
#     self.exit = False

#     def exitFunc():
#       return self.exit

#     def dataFunc(data,addr):
#       print('Got Data: {} from {}'.format(data, addr))

#     def threadFunc_():
#       while not self.exit:
#         if not self.connected:
#           self.socket = createSocket(self.port)
#           # connect

#           #     txSocket = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)

        
#         if self.connected:
#           udpRecv(self.port, dataFunc=dataFunc, exitFunc=exitFunc)

#         sleep(0.1)

#         # receive packet header (size)

#         # receive packet body

#     def threadFunc():
#       while not self.exit:
#         with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
#           s.bind((self.host, self.port))
#           s.listen()
#           conn, addr = s.accept()
          
#           self.connectionEvent(conn, addr)

#           # with con
#           #   if not data:
#           #     #  self.connectionEvent(conn, addr)

#           #     print('Connected by', addr)
#           #     self.connectionEvent(conn, addr)
#           #     # while True:
#           #     #     data = conn.recv(1024)
#           #     #     if not data:
#           #     #         break
#           #         # conn.sendall(data)
        
  
#     self.threadHandle = Thread(target=threadFunc)
#     self.threadHandle.start()

#   def stop(self):
#     self.exit = True
#     if self.threadHandle:
#       self.threadHandle.join()
#       self.threadHandle = None

