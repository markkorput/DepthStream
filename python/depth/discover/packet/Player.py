import logging
from time import time as getSysTime
logger = logging.getLogger(__name__)

class Player:
  def __init__(self, filepath, start=False, loop=True):
    self.filepath = filepath
    self.loop = loop

    self.file = None
    self.pendingFrame = None
    self.pendingFrameSysTime = None

    if start:
      self.start()

  def __del__(self):
    self.stop()

  def start(self):
    if not self.file:
      self.file = open(self.filepath, "rb")
      self.sysTimeOnPlay = getSysTime()
      self.playTimeOnPlay = 0.0  

  def stop(self):
    if self.file:
      self.file.close()
      self.file = None

  def update(self):
    # are we playing a file?
    if not self.file:
      return

    # no pending frame? try to read one
    if not self.pendingFrame:
      self.pendingFrame = Player.readFrame(self.file)

    # still no frame; we reached end of file
    if not self.pendingFrame:
      self._onEnd()
      return

    # make sure we have calculate the sys time at which frame should be played
    if not self.pendingFrameSysTime:
      timeMs = self.pendingFrame[0]
      
      timeSinceStartingPlayback = (timeMs * 0.001) - self.playTimeOnPlay
      self.pendingFrameSysTime = self.sysTimeOnPlay + timeSinceStartingPlayback

    if getSysTime() > self.pendingFrameSysTime:
      _, size, data = self.pendingFrame
      self.pendingFrame = None
      return (data, size)
  
  def _onEnd(self):
    if self.loop:
      self.stop()
      self.start()

  @classmethod
  def readFrame(cls, file):
    timeMs = int.from_bytes(file.read(4), byteorder='little')
    if not timeMs: return None
    size = int.from_bytes(file.read(4), byteorder='little')
    if not size: return None
    body = file.read(size)
    if not body: return None
    logger.info('Read frame from file: {}, {}'.format(timeMs, size))
    return (timeMs, size, body)


