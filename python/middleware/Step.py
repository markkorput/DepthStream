
class Step:
  def __init__(self, *args):
    self.args = args
  
  def then(self, func, onAbort=None):
    if not self.args: return self

    old = self.args
    res = func(*old)

    if type(res) == type(True):
      if not res:
        self.args = None
    else:
      self.args = res

    if not self.args and onAbort:
      onAbort(*old)

    return self

  @classmethod
  def explode(cls, args):
    if not args:
      return Step(args)
    return Step(*args)