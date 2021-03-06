import logging
logger = logging.getLogger(__name__)

#
# network
#

from discover.packet.Throttle import Throttle

def create_throttle(fps):
  return Throttle(fps=fps)


#
# compression
#

from discover.compress import decompress

def unzip(data, size):
  decomp = decompress(data, size)
  return (decomp, len(decomp)) if decomp else None

def log_unzip(data, size):
  logger.info('Unzipped frame into {} bytes'.format(size))
  return True

def log_unzip_failure(data, size):
  logger.warn('Failed to decompress packet of {} bytes'.format(size))
  return True

def log_packet_size(data, size):
  logger.info('Packet size: {}'.format(size))
  return True

#
# image conversion
#

import cv2
from .frame_sizes import to_frame, convert_16u_to_8u as c16u_to_8u

def load_grayscale_image(data, size):
  frame = to_frame(data, size)
  return (frame, size)

def convert_16u_to_8u(data, size):
  data = c16u_to_8u(data)
  return (data, size)

def create_16u_to_8u_converter(min, max):
  def func(data, size):
    data = c16u_to_8u(data, min=min, max=max)
    return (data,size)
  return func

def show(frame, size):
  cv2.imshow('playback {}'.format(frame.shape), frame)
  return True