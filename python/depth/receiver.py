#!/usr/bin/python

import socket
from threading import Thread
from time import sleep
import sys


def udpRecv(portNum, dataFunc=None, exitFunc=None):
  #Generate a UDP socket
  rxSocket = socket.socket(socket.AF_INET, #Internet
    socket.SOCK_DGRAM) #UDP
                            
  #Bind to any available address on port *portNum*
  rxSocket.bind(("",portNum))
  
  #Prevent the socket from blocking until it receives all the data it wants
  #Note: Instead of blocking, it will throw a socket.error exception if it
  #doesn't get any data
  
  rxSocket.setblocking(0)
  
  # print "RX: Receiving data on UDP port " + str(portNum)
  # print ""
  
  if exitFunc == None:
    def never():
      return False

  while not exitFunc():
      try:
          #Attempt to receive up to 1024 bytes of data
          data,addr = rxSocket.recvfrom(1024) 
          #Echo the data back to the sender
          # rxSocket.sendto(str(data),addr)
          # print('received {} from {}'.format(data, addr)) # => received b'/abc\x00\x00\x00\x00,\x00\x00\x00' from ('127.0.0.1', 63257)
          if dataFunc:
            dataFunc(data, addr)

      except socket.error:
          #If no data is received, you get here, but it's not an error
          #Ignore and continue
          pass

      sleep(.1)

# class UdpReceiver:
#   def __init__(self, port=8000):
#     self.exit = False
#     self.port = port

#   def threadFunc(self, portNum):
#     #Generate a UDP socket
#     rxSocket = socket.socket(socket.AF_INET, #Internet
#       socket.SOCK_DGRAM) #UDP
                              
#     #Bind to any available address on port *portNum*
#     rxSocket.bind(("",portNum))
    
#     #Prevent the socket from blocking until it receives all the data it wants
#     #Note: Instead of blocking, it will throw a socket.error exception if it
#     #doesn't get any data
    
#     rxSocket.setblocking(0)
    
#     # print "RX: Receiving data on UDP port " + str(portNum)
#     # print ""
    
#     while not self.exit:
#         try:
#             #Attempt to receive up to 1024 bytes of data
#             data,addr = rxSocket.recvfrom(1024) 
#             #Echo the data back to the sender
#             # rxSocket.sendto(str(data),addr)
#             print('received {} from {}'.format(data, addr)) # => received b'/abc\x00\x00\x00\x00,\x00\x00\x00' from ('127.0.0.1', 63257)

#         except socket.error:
#             #If no data is received, you get here, but it's not an error
#             #Ignore and continue
#             pass

#         sleep(.1)

#   def main(self):    
#     udpRxThreadHandle = Thread(target=self.threadFunc,args=(self.port,))
#     udpRxThreadHandle.start()


#     sleep(.1)
    
#     #Generate a transmit socket object
#     txSocket = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
    
#     #Do not block when looking for received data (see above note)
#     txSocket.setblocking(0) 
   
#     # print "Transmitting to 127.0.0.1 port " + str(portNum)
#     # print "Type anything and press Enter to transmit"
#     while True:
#         try:
#              #Retrieve input data 
#             # txChar = raw_input("TX: ")
            
#             #Transmit data to the local server on the agreed-upon port
#             # txSocket.sendto(txChar,("127.0.0.1",portNum))
            
#             #Sleep to allow the other thread to process the data
#             sleep(.2)
            
#             #Attempt to receive the echo from the server
#             data, addr = txSocket.recvfrom(1024)
            
#             # print "RX: " + str(data) 

#         except socket.error as msg:    
#             #If no data is received you end up here, but you can ignore
#             #the error and continue
#             pass   
#         except KeyboardInterrupt:
#             self.exit = True
#             # print("Received Ctrl+C... initiating exit")
#             break
#         sleep(.1)
         
#     udpRxThreadHandle.join()
        
#     return

class Receiver:
  def __init__(self, port=4445):
    self.port = port
    self.threadHandle = None
    self.exit = False
    self.connect = False

  def start(self):
    self.exit = False

    def exitFunc():
      return self.exit

    def dataFunc(data,addr):
      print('Got Data: {} from {}'.format(data, addr))

    def threadFunc():
      while not self.exit:
        if not self.connected:
          # connect

          #     txSocket = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)

        
        if not self.connected:
          sleep(0.1)

        # receive packet header (size)

        # receive packet body

        udpRecv(port, dataFunc=dataFunc, exitFunc=exitFunc)
    
    self.threadHandle = Thread(target=threadFunc)
    self.threadHandle.start()

  def stop(self):
    self.exit = True
    if self.threadHandle:
      self.threadHandle.join()


  def funcname(self, parameter_list):
    #Generate a UDP socket
    rxSocket = socket.socket(socket.AF_INET, #Internet
    socket.SOCK_DGRAM) #UDP
                            
    #Bind to any available address on port *portNum*
    rxSocket.bind(("",portNum))
    
    #Prevent the socket from blocking until it receives all the data it wants
    #Note: Instead of blocking, it will throw a socket.error exception if it
    #doesn't get any data
    
    # rxSocket.setblocking(0)
    
    # print "RX: Receiving data on UDP port " + str(portNum)
    # print ""
    
    while not self.exit:
        try:
            #Attempt to receive up to 1024 bytes of data
            data,addr = rxSocket.recvfrom(1024) 
            #Echo the data back to the sender
            # rxSocket.sendto(str(data),addr)
            # print('received {} from {}'.format(data, addr)) # => received b'/abc\x00\x00\x00\x00,\x00\x00\x00' from ('127.0.0.1', 63257)
            if dataFunc:
              dataFunc(data, addr)

        except socket.error:
            #If no data is received, you get here, but it's not an error
            #Ignore and continue
            pass

        sleep(.1)
if __name__ == '__main__':
  # r = UdpReceiver(int(sys.argv[1]) if len(sys.argv) > 1 else 8000)
  # r.main()


  port = int(sys.argv[1]) if len(sys.argv) > 1 else 8000

  r = Receiver(port=port)
  r.start()
  try:
    while True:
      sleep(0.3)
  except KeyboardInterrupt:
    r.stop()

    print("Received Ctrl+C... initiating exit")
    
  sleep(.1)
  
