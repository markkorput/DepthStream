#!/usr/bin/python
import logging, sys, cv2
from threading import Thread
from time import sleep
from time import time as getSysTime
from optparse import OptionParser
from discover.socket import ClientThread, PacketStreamInfo, appendPacketsIntoList
from discover.packet.Buffer import Buffer
from middleware import Step
from .steps import unzip, log_packet_size, log_unzip, log_unzip_failure, load_grayscale_image, show

logger = logging.getLogger(__name__)

class IntervalTimer:
  def __init__(self, interval):
    # self.fps = fps
    self.interval = interval
    self.nextTime = getSysTime()
  
  def check(self):
    t = getSysTime()
    if t < self.nextTime:
      return False
    self.nextTime = t + self.interval
    return True

def parse_args():
  parser = OptionParser()
  parser.add_option('-p', '--port', dest="port", default=4445, type='int')
  parser.add_option('--host', dest="host", default='127.0.0.1')
  parser.add_option('-s', '--show', dest="show", action='store_true', default=False)

  parser.add_option('-v', '--verbose', dest="verbose", action='store_true', default=False)
  parser.add_option('--verbosity', dest="verbosity", action='store_true', default='info')

  opts, args = parser.parse_args()
  lvl = {'debug': logging.DEBUG, 'info': logging.INFO, 'warning':logging.WARNING, 'error':logging.ERROR, 'critical':logging.CRITICAL}['debug' if opts.verbose else str(opts.verbosity).lower()]
  logging.basicConfig(level=lvl)

  return opts, args

if __name__ == '__main__':
  opts, args = parse_args()

  infoTimer = IntervalTimer(2.0)
  streamInfo = None

  queue = []

  # our socket thread takes care of connecting to the specified server. When
  # a connection has been established, it passes control to our receiver
  ct = ClientThread(opts.host, opts.port,
    # once connection has been established this receiver will
    # continuously read packet into our buffer
    connectionFunc=appendPacketsIntoList(queue))

  # register some callbacks for feedback about connection status
  def onConnect(socket): logger.info('Connected')
  def onDisconnect(socket): logger.info('Disconnected')
  ct.connectEvent += onConnect
  ct.disconnectEvent += onDisconnect

  def checkInfo(data, size):
    global streamInfo

    info_data = PacketStreamInfo.parse(data, size)
    if info_data:
      logger.info('Got stream info: {}'.format(data))
      streamInfo = PacketStreamInfo(info_data)

    if streamInfo == None and infoTimer.check():
      sock = ct.getSocket()
      if not sock:
        return True

      logger.info('Sending stream info request')
      PacketStreamInfo.sendRequest(sock)

    return True

  try:
    keepGoing = True

    while keepGoing:
      if len(queue) < 1:
        sleep(0.3)
        continue

      packet = queue[0]
      del queue[0]

      # process frame; decompress, then log decompress information
      step = Step(*packet).sequence([
        # log_packet_size,
        checkInfo
      ]).then(unzip, onAbort=log_unzip_failure).then(log_unzip)

      # IF the show option is enabled, further process the packet by converting
      # it into a grayscale image and showing it
      if opts.show:
        step.sequence([
          load_grayscale_image,
          show
        ])

      key = cv2.waitKey(20) & 0xFF
      if key == 27 or key == ord('q'): # escape or Q
        keepGoing = False

  except KeyboardInterrupt:
    print("Received Ctrl+C... initiating exit")

  ct.stop()
  sleep(.1)
  
