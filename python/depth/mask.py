#!/usr/bin/python
import logging, cv2
from time import sleep
from optparse import OptionParser

import numpy as np
from discover.socket import SocketConnectionAccepter, PacketService
from discover.packet.Player import Player
from discover.packet.Buffer import Buffer
from middleware import Step
from .steps import unzip, log_unzip, log_unzip_failure, show, create_throttle, create_throttle, load_grayscale_image, convert_16u_to_8u, create_16u_to_8u_converter


logger = logging.getLogger(__name__)

if __name__ == '__main__':
  parser = OptionParser()
  parser.add_option('-p', '--port', dest="port", default=4445, type='int')
  parser.add_option('--fps', dest="fps", default=25.0, type='float')
  parser.add_option('-f', '--file', dest="file", default=None)
  parser.add_option('-s', '--show', dest="show", action='store_true', default=False)
  parser.add_option('--processor', dest="processor", default=None)
  parser.add_option('-v', '--verbose', dest="verbose", action='store_true', default=False)

  parser.add_option('--verbosity', dest="verbosity", action='store_true', default='info')

  opts, args = parser.parse_args()

  lvl = {'debug': logging.DEBUG, 'info': logging.INFO, 'warning':logging.WARNING, 'error':logging.ERROR, 'critical':logging.CRITICAL}['debug' if opts.verbose else str(opts.verbosity).lower()]
  logging.basicConfig(level=lvl)

  filepath = opts.file if opts.file else args[0] if len(args) > 0 else None

  # this buffer will hold the data buffer and packet size for every incoming frame
  buffer = Buffer()
  player = Player(filepath, start=True)


  # server = SocketConnectionAccepter(opts.port)

  # # todo; (re-)initialize service with actual server port
  # service = None if opts.show else PacketService("packetframes", server.port,
  #   infoData={'playback': filepath})

  # packet processing step: throttle
  throttle = create_throttle(opts.fps)
  # packet processing step: bit convert 16 bits to 8 bits
  bit_converter = create_16u_to_8u_converter(0, 1000)

  # packet processing step: processor (image effects)
  processor = None

  if opts.show and opts.processor:
    from .processor import create_controlled_processor_from_json_file #, create_processor_from_json_file
    p = create_controlled_processor_from_json_file(opts.processor, winid='ctrl')

    def func(frame,size):
      f = p(frame)
      return (f,size)

    processor = func


  class Masker:
    def __init__(self):
      self.mask = None

    def add(self, data, size):
      if type(self.mask) == type(None):
        self.mask = np.array(data, copy=True)
      else:     
        self.mask = np.maximum(data, self.mask)

      cv2.imshow('mask', self.mask)

    def clear(self):
      self.mask = None

    
    def masked(self, img):
      if type(self.mask) == type(None):
        return img

      less = np.less(img, self.mask)
      return img - img * less
      # return np.ma.masked_array(img, mask=np.less(img, self.mask)) if type(self.mask) != type(None) else img
      # return img - self.mask if type(self.mask) != type(None) else img

  
  masker = Masker()

  def masked(img, size):
    return masker.masked(img), size

  def record(data, size):
    Step(data, size).sequence([
      masker.add
    ])

    return True


  # packet processing step: showing frame to user
  def show_frame(data, size):
    Step(data, size).sequence([
      masked,
      show
    ])


  ### Record controls
  isRecording = False
  isClearing = False
  def onRecordChange(val):
    global isRecording
    isRecording = (val == 1)
    logger.info(f'isRecording: {isRecording}')

  def onClearChange(val):
    global isClearing
    isClearing = (val == 1)

  ctrlid = "RecordControls"
  cv2.namedWindow(ctrlid)
  cv2.createTrackbar('Record', ctrlid, 0, 1, onRecordChange)
  cv2.createTrackbar('Clear', ctrlid, 0, 1, onClearChange)

  # main loop
  try:
    while True:
      # # update our server add pass any new incoming connections on to our service
      # server.update(onConnection=service.addConsumerSocket if service else None)

      if isClearing:
        masker.clear()

      # # process incoming data
      # if service:
      #   service.update() 

      # check if our player has any new packets
      packet = player.update()

      if packet:
        data, size = packet

        # Step(data, size).then(throttle).then(unzip, onAbort=logUnzipFailure).then(logUnzip).then(show_frame)
        Step(data, size).sequence([
          throttle,
          unzip, # else log_unzip_failure
          load_grayscale_image,
          # service.submit if service else None,
          record if isRecording == 1 else None,
          show_frame if opts.show else None])

      if opts.show:
        key = cv2.waitKey(20) & 0xFF
        if key == 27 or key == ord('q'): # escape or Q
          break

      sleep(0.1)
  except KeyboardInterrupt:
    print("Received Ctrl+C... initiating exit")

  player.stop()
  server.stop()

  sleep(.1)
  
