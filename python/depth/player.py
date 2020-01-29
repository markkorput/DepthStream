#!/usr/bin/python
import logging, sys, cv2
from threading import Thread
from time import sleep, time
from optparse import OptionParser
import numpy as np

from discover.socket import PacketService
from discover.packet.Player import Player
from discover.packet.Buffer import Buffer
from discover.packet.Throttle import Throttle
from discover.compress import decompress

from middleware.Step import Step
from .frame_sizes import to_frame

logger = logging.getLogger(__name__)

if __name__ == '__main__':
  parser = OptionParser()
  parser.add_option('-p', '--port', dest="port", default=4445, type='int')
  parser.add_option('--fps', dest="fps", default=25.0, type='float')
  parser.add_option('-f', '--file', dest="file", default=None)
  parser.add_option('-s', '--show', dest="show", action='store_true', default=False)
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

  service = None if opts.show else PacketService("packetframes", opts.port)

  def unzip(data, size):
    decomp = decompress(data, size)
    return (decomp, len(decomp)) if decomp else None

  def logUnzip(data, size):
    logger.info('Unzipped frame into {} bytes'.format(size))
    return True

  def logUnzipFailure(data, size):
    logger.warn('Failed to decompress packet of {} bytes'.format(size))

  def load_grayscale_image(data, size):
    frame = to_frame(data, size)
    return (frame, size)

  def show(frame, size):
    cv2.imshow('playback {}'.format(frame.shape), frame)
    return False

  def show_frame(data, size):
    Step(data, size).sequence([
      unzip,
      # logUnzip,
      load_grayscale_image,
      show
    ])

  try:
    while True:
      
      frame = player.update()
      if frame:
        data, size = frame

        # Step(data, size).then(throttle).then(unzip, onAbort=logUnzipFailure).then(logUnzip).then(show_frame)
        Step(data, size).sequence([
          throttle,
          service.submit if service else None,
          show_frame if opts.show else None])

      key = cv2.waitKey(20) & 0xFF
      if key == 27 or key == ord('q'): # escape or Q
        break

      sleep(0.1)
  except KeyboardInterrupt:
    print("Received Ctrl+C... initiating exit")

  player.stop()
  if service:
    service.stop()

  sleep(.1)
  
