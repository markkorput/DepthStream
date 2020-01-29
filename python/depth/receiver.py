#!/usr/bin/python
import logging, sys
from threading import Thread
from time import sleep
from optparse import OptionParser

from .discover.socket import SocketClientThread, PacketStreamReceiver
from .discover.compress import compress, decompress
from .Buffer import Buffer
logger = logging.getLogger(__name__)

if __name__ == '__main__':
  parser = OptionParser()
  parser.add_option('-p', '--port', dest="port", default=4445, type='int')
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
  ct = SocketClientThread('127.0.0.1', opts.port, connectionFunc=receiver.receive)

  def onConnect(s): logger.info('Connected')
  def onDisconnect(s): logger.info('Disconnected')

  ct.connectEvent += onConnect
  ct.disconnectEvent += onDisconnect

  try:
    while True:
      frame = buffer.read()
      if frame:
        # logging.info('Got frame')
        data, size = frame
        decomp = decompress(data, size)
        logger.info('size (compressed/decompressed): {}/{}'.format(size, len(decomp)))

      sleep(0.3)
  except KeyboardInterrupt:
    print("Received Ctrl+C... initiating exit")

  receiver.stop()
  ct.stop()    

  sleep(.1)
  
