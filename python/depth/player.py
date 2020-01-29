#!/usr/bin/python
import logging, sys
from threading import Thread
from time import sleep, time
from optparse import OptionParser

from discover.socket import PacketService
from discover.packet.Player import Player
from middleware.Step import Step

from .Buffer import Buffer

logger = logging.getLogger(__name__)

class Timer:
  def __init__(self, fps):
    self.fps = fps
    self.interval = 1.0 / self.fps
    self.nextTime = time()
  
  def check(self):
    t = time()
    if t < self.nextTime:
      return False
    self.nextTime = t + self.interval
    return True
  
class Throttle:
  def __init__(self, fps):
    self.timer = Timer(fps)
  
  def process(self, data, size):
    return self.timer.check()

  __call__ = process

if __name__ == '__main__':
  parser = OptionParser()
  parser.add_option('-p', '--port', dest="port", default=4445, type='int')
  parser.add_option('--fps', dest="fps", default=25.0, type='float')
  parser.add_option('-f', '--file', dest="file", default=None)
  parser.add_option('-v', '--verbose', dest="verbose", action='store_true', default=False)

  parser.add_option('--verbosity', dest="verbosity", action='store_true', default='info')

  opts, args = parser.parse_args()

  lvl = {'debug': logging.DEBUG, 'info': logging.INFO, 'warning':logging.WARNING, 'error':logging.ERROR, 'critical':logging.CRITICAL}['debug' if opts.verbose else str(opts.verbosity).lower()]
  logging.basicConfig(level=lvl)

  filepath = opts.file if opts.file else args[0] if len(args) > 0 else None

  # this buffer will hold the data buffer and packet size for every incoming frame
  buffer = Buffer()
  player = Player(filepath, start=True)

  throttle = Throttle(fps=opts.fps)
  service = PacketService("packetframes", opts.port)

  try:
    while True:
      
      frame = player.update()
      if frame:
        # logging.info('Got frame')
        data, size = frame

        Step(data, size).then(throttle).then(service.submit)

      sleep(0.1)
  except KeyboardInterrupt:
    print("Received Ctrl+C... initiating exit")

  player.stop()
  service.stop()
  # ct.stop()    

  sleep(.1)
  
