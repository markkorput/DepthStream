from time import time as getSysTime

class Timer:
  def __init__(self, fps):
    # self.fps = fps
    self.interval = 1.0 / fps
    self.nextTime = getSysTime()
  
  def check(self):
    t = getSysTime()
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
