class Buffer:
  def __init__(self):
    self.data = None

  def write(self, data):
    self.data = data

  def read(self, clear=True):
    d = self.data
    if clear: self.clear()
    return d

  def write_buffer_and_size(self, buffer, size):
    self.data = (buffer,size)

  def clear(self):
    self.data = None