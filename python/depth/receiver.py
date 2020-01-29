#!/usr/bin/python
import logging, sys
from threading import Thread
from time import sleep
from optparse import OptionParser

from discover.socket import SocketClientThread, PacketStreamReceiver
from discover.compress import compress, decompress
from discover.packet.Buffer import Buffer
from middleware.Step import Step

logger = logging.getLogger(__name__)

if __name__ == '__main__':
  parser = OptionParser()
  parser.add_option('-p', '--port', dest="port", default=4445, type='int')
  parser.add_option('--host', dest="host", default='127.0.0.1')
  parser.add_option('-v', '--verbose', dest="verbose", action='store_true', default=False)
  parser.add_option('--verbosity', dest="verbosity", action='store_true', default='info')

  opts, args = parser.parse_args()

  lvl = {'debug': logging.DEBUG, 'info': logging.INFO, 'warning':logging.WARNING, 'error':logging.ERROR, 'critical':logging.CRITICAL}['debug' if opts.verbose else str(opts.verbosity).lower()]
  logging.basicConfig(level=lvl)

  # this buffer will hold the data buffer and packet size for every incoming frame
  buffer = Buffer()
  # the receiver will continuously read packet-header and packet-body and pass the
  # packet bodies, into our buffer
  receiver = PacketStreamReceiver(buffer.write_buffer_and_size)
  # our socket thread 
  ct = SocketClientThread(opts.host, opts.port, connectionFunc=receiver.receive)

  def onConnect(socket): logger.info('Connected')
  def onDisconnect(socket): logger.info('Disconnected')
  ct.connectEvent += onConnect
  ct.disconnectEvent += onDisconnect


  def unzip(data, size):
    decomp = decompress(data, size)
    return (decomp, len(decomp)) if decomp else None

  def logCompression(originalPacket):
    def logC(data, size):
      logger.info('Got packet, size (compressed/decompressed): {}/{}'.format(originalPacket[1], size))
    return logC 

  def logFailure(data, size):
    logger.warn('Failed to decompress packet of {} bytes'.format(size))

  try:
    while True:

      frame = buffer.read()
      if frame:
        Step(*frame).then(unzip, onAbort=logFailure).then(logCompression(frame))

      sleep(0.3)
  except KeyboardInterrupt:
    print("Received Ctrl+C... initiating exit")

  receiver.stop()
  ct.stop()    

  sleep(.1)
  
