
class Step:
  def __init__(self, *args):
    self.args = args
  
  def then(self, func, onAbort=None):
    if type(self.args) == type(None): return self

    old = self.args
    res = func(*old)

    if type(res) == type(True):
      if not res:
        self.args = None
    else:
      self.args = res

    if type(self.args) == type(None) and onAbort:
      onAbort(*old)

    return self

  def sequence(self, steps):
    cur = self
    for action in steps:
      if action != None:
        cur = cur.then(action)

    return cur

  @classmethod
  def explode(cls, args):
    if not args:
      return Step(args)
    return Step(*args)