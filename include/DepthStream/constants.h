#pragma once

#include <cstddef>

namespace depth {
  static const size_t FRAME_SIZE_640x480x16BIT = (640*480*2); // orbbec
  static const size_t FRAME_SIZE_640x480x32BIT = (640*480*4);
  static const size_t FRAME_SIZE_512x424x32BIT = (512*424*4); // kinect
  static const size_t FRAME_SIZE_512x424x16BIT = (512*424*2); // kinect
  static const size_t FRAME_SIZE_640x240x08BIT = (640*240*1); // leap motion
  static const size_t FRAME_SIZE_1280x720x16BIT = (1280*720*2); // Intel RealSense D415/D435
}
