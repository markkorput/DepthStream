#!/usr/bin/python
import logging, cv2
from time import sleep
from optparse import OptionParser

from discover.socket import SocketConnectionAccepter, PacketService
from discover.packet.Player import Player
from discover.packet.Buffer import Buffer
from middleware import Step
from .steps import unzip, log_unzip, log_unzip_failure, show, create_throttle, create_throttle, load_grayscale_image, convert_16u_to_8u, create_16u_to_8u_converter
from remote_params import Params, Server, OscServer

logger = logging.getLogger(__name__)

def create_processor(processor_file_path, winid='controls'):
  from .processor import create_controlled_processor_from_json_file #, create_processor_from_json_file
  p = create_controlled_processor_from_json_file(processor_file_path, winid=winid)

  if not p:
    return None

  def func(frame,size):
    f = p(frame)
    return (f,size)

  return func
  
def create_params():
  params = Params()
  param = params.bool('effects')
  return params

if __name__ == '__main__':
  parser = OptionParser()
  parser.add_option('-p', '--port', dest="port", default=4445, type='int')
  parser.add_option('--fps', dest="fps", default=25.0, type='float')
  parser.add_option('-f', '--file', dest="file", default=None)
  parser.add_option('-s', '--show', dest="show", action='store_true', default=False) 
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

  server = SocketConnectionAccepter(opts.port) if not opts.show else None

  # todo; (re-)initialize service with actual server port
  service = PacketService("packetframes", server.port,
    infoData={'playback': filepath}) if server else None

  # packet processing step: throttle
  throttle = create_throttle(opts.fps) if server else None
  # packet processing step: bit convert 16 bits to 8 bits
  bit_converter = create_16u_to_8u_converter(0, 1000)

  # packet processing step: processor (image effects)
  processor = create_processor(opts.processor) if opts.show and opts.processor else None


  params = create_params()
  osc_server = OscServer(Server(params))

  # packet processing step: showing frame to user
  def show_frame(data, size):
    Step(data, size).sequence([
      unzip, # else log_unzip_failure
      # log_unzip,
      load_grayscale_image,
      bit_converter if params.get('effects').val() else None,
      # processor, # ony loaded if --processor arg specified a valid file
      # processor,
      show
    ])

  # main loop
  try:
    while True:
      # update our server add pass any new incoming connections on to our service
      if server: server.update(onConnection=service.addConsumerSocket)

      # process incoming data from consumers (like stream info requests)
      if service: service.update() 

      # check if our player has any new packets
      packet = player.update()

      if packet:
        data, size = packet

        # Step(data, size).then(throttle).then(unzip, onAbort=logUnzipFailure).then(logUnzip).then(show_frame)
        Step(data, size).sequence([
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
  if server: server.stop()

  sleep(.1)
  
