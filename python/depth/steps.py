import cv2, logging, sys
from discover.compress import decompress
from discover.packet.Throttle import Throttle
from .frame_sizes import to_frame, convert_16u_to_8u as c16u_to_8u

logger = logging.getLogger(__name__)

def unzip(data, size):
  decomp = decompress(data, size)
  return (decomp, len(decomp)) if decomp else None

def log_unzip(data, size):
  logger.info('Unzipped frame into {} bytes'.format(size))
  return True

def log_unzip_failure(data, size):
  logger.warn('Failed to decompress packet of {} bytes'.format(size))
  return True

def load_grayscale_image(data, size):
  frame = to_frame(data, size)
  return (frame, size)

def convert_16u_to_8u(data, size):
  data = c16u_to_8u(data)
  return (data, size)

def show(frame, size):
  cv2.imshow('playback {}'.format(frame.shape), frame)
  return True

def create_throttle(fps):
  return Throttle(fps=fps)