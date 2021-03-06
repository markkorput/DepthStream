#!/usr/bin/python
import logging, cv2, os
from time import sleep
from optparse import OptionParser

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
  parser.add_option('--save-frames-to', dest="save_frames_to", default=None)
  parser.add_option('--processor', dest="processor", default='data/process_player.json')
  parser.add_option('-v', '--verbose', dest="verbose", action='store_true', default=False)

  parser.add_option('--verbosity', dest="verbosity", action='store_true', default='info')

  opts, args = parser.parse_args()

  lvl = {'debug': logging.DEBUG, 'info': logging.INFO, 'warning':logging.WARNING, 'error':logging.ERROR, 'critical':logging.CRITICAL}['debug' if opts.verbose else str(opts.verbosity).lower()]
  logging.basicConfig(level=lvl)

  filepath = opts.file if opts.file else args[0] if len(args) > 0 else None

  # this buffer will hold the data buffer and packet size for every incoming frame
  buffer = Buffer()
  player = Player(filepath, start=True)

  server = SocketConnectionAccepter(opts.port)

  # todo; (re-)initialize service with actual server port
  service = None if opts.show else PacketService("packetframes", server.port,
    infoData={'playback': filepath})

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

  # packet processing step: showing frame to user
  def show_frame(data, size):
    Step(data, size).sequence([
      unzip, # else log_unzip_failure
      # log_unzip,
      load_grayscale_image,
      # bit_converter,
      # processor, # ony loaded if --processor arg specified a valid file
      processor,
      show
    ])


  save_frame_index = 0
  def save_frame(compressed_data, compressed_size):
    global save_frame_index
    data, size = unzip(compressed_data, compressed_size)
    # save_frame(data)

    number = str(save_frame_index)
    while len(number) < 6:
      number = '0' + number

    file_path = os.path.join(opts.save_frames_to, f'{number}.png')

    logger.info(f'Writing frame: {file_path}')
    img, _ = load_grayscale_image(data, size)
    cv2.imwrite(file_path, img)
    
    save_frame_index += 1
    return compressed_data, compressed_size

  # main loop
  try:
    while True:
      # update our server add pass any new incoming connections on to our service
      server.update(onConnection=service.addConsumerSocket if service else None)

      # process incoming data
      if service:
        service.update() 

      # check if our player has any new packets
      packet = player.update()

      if packet:
        data, size = packet

        # Step(data, size).then(throttle).then(unzip, onAbort=logUnzipFailure).then(logUnzip).then(show_frame)
        Step(data, size).sequence([
          save_frame if opts.save_frames_to else None,
          throttle,
          service.submit if service else None,
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
  
