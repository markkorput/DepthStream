
FRAME_SIZE_640x480x16BIT = (640*480*2) # orbbec
FRAME_SIZE_640x480x32BIT = (640*480*4) 
FRAME_SIZE_512x424x32BIT = (512*424*4) # kinect
FRAME_SIZE_512x424x16BIT = (512*424*2) # kinect
FRAME_SIZE_640x240x08BIT = (640*240*1) # leap motion
FRAME_SIZE_1280x720x16BIT = (1280*720*2) # Intel RealSense D415/D435

import numpy as np

def to_frame(data, size):
  if size == FRAME_SIZE_1280x720x16BIT:
    frame = np.frombuffer(data, dtype='<u2')
    frame = (frame - 500 / (3500-500))
    frame = frame.reshape(720, 1280)
    return frame

  if size == FRAME_SIZE_640x480x16BIT:
    frame = np.frombuffer(data, dtype='<u2')
    frame = 1.0 - frame / 4000
    frame = frame.reshape(480, 640)
    return frame

  logger.warn('Unsupported frame size: {} bytes'.format(size))
  return None


def convert_16u_to_8u(image):
  def display(image, display_min, display_max): # copied from Bi Rico
    # Here I set copy=True in order to ensure the original image is not
    # modified. If you don't mind modifying the original image, you can
    # set copy=False or skip this step.
    image = np.array(image, copy=True)
    image.clip(display_min, display_max, out=image)
    image -= display_min
    np.floor_divide(image, (display_max - display_min + 1) / 256,
                    out=image, casting='unsafe')
    return image.astype(np.uint8)

  def lut_display(image, display_min, display_max) :
    lut = np.arange(2**16, dtype='uint16')
    lut = display(lut, display_min, display_max)
    return np.take(lut, image)
  
  converted = display(image, 1000, 10000)
  return converted