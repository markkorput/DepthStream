# this file is copied from github.com/markkorput/pyStereoVision's utils folder

import os, logging, json, cv2
import numpy as np

def isNone(var):
  '''
  utility method to be used with variables that might carry nparray values, which are a bit of a pain when comparing to None
  '''
  return type(var) == type(None)

def addParamTrackbar(winid, params, param, max=None, initialValue=None, valueProc=None, values=None, factor=None, readProc=None, onChange=None, controlId=None, default=None):
  '''
  Convenience method for param manipulating trackbars

  Args:
    winid (str): opencv window ID
    params (dict): a dict (with string-based keys) containing the params
    max (int): maximum value
    initialValue (int): initialValue for the trackbar
    valueProc (method): a value pre-processor method that takes the trackbar value and return the value to be applied to the param
    readProc (method): a value pre-processor method that takes the param value and return trackbar value for the initial value
    values (list): a list of possible values. When specified, this will overwrite the max, valueProx and initialValue values
    factor (float): a multiplier/division value
  '''

  if param in params:
    default = params[param] 
  if isNone(default):
    default = 0

  if factor:
    max = factor
    valueProc = lambda v: float(v) / factor
    readProc = lambda v: int(v * factor)

  if values:
    max = len(values)-1
    valueProc = lambda v: values[v]
    if initialValue == None:
      initialValue = values.index(default) if default in values else values[0]
      # print("{} int val: {} for def: {} in values: {}".format(param, initialValue, default, values))

  if not readProc:
    readProc = lambda v: int(v)

  def onValue(val):
    params[param] = valueProc(val) if valueProc else val
    if onChange:
      onChange()

  val = initialValue if initialValue != None else readProc(params[param] if param in params else default)
  cv2.createTrackbar(controlId if controlId else param, winid, val, max, onValue)


def createParamsGuiWin(winid, params, file=None, load=None, save=None):
  class Builder:
    def __init__(self, winid, params, file=None, load=None, save=None):
      self.winid = winid
      self.params = params
      self.file = file
      self.load = load
      self.save = save

      cv2.namedWindow(self.winid)
      # cv2.moveWindow(self.winid, 5, 5)
      # cv2.resizeWindow(self.winid, 500,400)

    # def __del__(self):
    #   if self.file and self.save != False:
    #     self._save()

    def __enter__(self):
      if self.file and self.load != False:
        self._load()  

      return self

    def __exit__(self, type, value, traceback):
      pass

    def _load(self):
      # save params to file
      logging.info('Loading params for gui win {} to file: {}'.format(self.winid, self.file))
      json_data = {}
      with open(file, 'r') as f:
        json_data = json.load(f)
      self.params.update(json_data)

    # def _save(self):
    #   # save params to file
    #   logging.info('Writing params for gui win {} to file: {}'.format(self.winid, self.file))
    #   with open(file, 'w') as f:
    #     json.dump(self.params, f)

    def add(self, paramname, *args, **kwargs):
      return addParamTrackbar(self.winid, self.params, paramname, *args, **kwargs)

  # create and return Builder instance 
  return Builder(winid, params, file, load, save)





def _get_data_from_json_file(filepath):
  '''
  Tries to load json data form the specified filepath.
  In case of failure, None is returned.
  '''

  # config file exists?
  if not os.path.isfile(filepath):
    return None

  # read file content
  text = None
  with open(filepath, "r") as f:
      text = f.read()

  # parse json
  data = None
  try:
    data = json.loads(text)
  except json.decoder.JSONDecodeError as err:
    logging.warning('Could not load calibration json: \n{}'.format(err))
    return None

  return data

def _create_wrapping_processor(funcs):
  def wrapping_func(frame, opts={}):
    f = frame
    for func in funcs:
      f = func(f, opts)
    return f

  # return wrapping processor func
  return wrapping_func

def create_processor(data):
  '''
  Creates a processor based on the given data.
  A processor is just a method that takes a single frame as input and returns a processed frame as output.
  '''
  typ = data['type'] if 'type' in data else None
  
  def enhance(func):
    '''
    enhances a function with verbosity (logging) and enabled options
    '''
    verbose = data['verbose'] if 'verbose' in data else False
    enabled = not ('enabled' in data and data['enabled'] == False)

    # this finalfunc wraps around the given func, adding 'enabled' and 'verbose' options
    def finalfunc(frame, opts={}):
      # if not enabled, no processing to the frame is required, just return the original frame
      if not enabled: return frame
      # verbosity: log processor activity
      if verbose: print('[fx] {}'.format(typ))
      return func(frame, opts)

    return finalfunc

  def select(val, values, aliases=[]):
    '''
    Selects (returns) one of the givens values based on the specified val, which can be either an
    index, or an alias value (if aliases are specified)
    '''
    if val in aliases:
      val = aliases.index(val)
    elif type(val) == type('') and val.isdigit():
      val = int(val)

    if type(val) == type(0):
      if val < 0 or val >= len(values):
        return values[0]
      return values[val]

    return values[0]

  def select_int(val, min=None, max=None):
    val = int(val)
    if min != None and val < min:
      return min
    if max != None and val > max:
      return max
    return val

  def select_float(val, min=None, max=None):
    val = float(val)
    if min != None and val < min:
      return min
    if max != None and val > max:
      return max
    return val

  if typ == 'grayscale':
    def func(f, opts={}):
      return cv2.cvtColor(f,cv2.COLOR_BGR2GRAY)
    return enhance(func)

  if typ == 'invert':
    def func(f, opts={}):
      return 255-f
    return enhance(func)

  if typ == 'blur':
    x = max(data['x'] if 'x' in data else 1, 1)
    y = max(data['y'] if 'y' in data else 1, 1)
    def func(f, opts={}):
      return cv2.Blur(f, (x,y))
    return enhance(func)

  if typ == 'gaussianblur':
    x = data['x'] if 'x' in data else 0.0
    y = data['y'] if 'y' in data else 0.0
    sx = data['sigma-x'] if 'sigma-x' in data else 10
    sy = data['sigma-y'] if 'sigma-y' in data else 10

    def func(f, opts={}):
      return cv2.GaussianBlur(f, (x,y), sx,sy)
    return enhance(func)


  # if typ == 'diff'

  if typ == 'threshold':
    value = data['value'] if 'value' in data else 0
    mx = data['max'] if 'max' in data else 255

    types = {
      'CHAIN_APPROX_NONE':cv2.CHAIN_APPROX_NONE,
      'CHAIN_APPROX_SIMPLE':cv2.CHAIN_APPROX_SIMPLE,
      'CHAIN_APPROX_TC89_L1':cv2.CHAIN_APPROX_TC89_L1,
      'CHAIN_APPROX_TC89_KCOS':cv2.CHAIN_APPROX_TC89_KCOS
    }
    threshold_type = types[data['method']] if 'method' in data and data['method'] in types else None
    if threshold_type == None and 'method' in data and type(data['method']) == type(1):
      types = [cv2.CHAIN_APPROX_NONE,cv2.CHAIN_APPROX_SIMPLE,cv2.CHAIN_APPROX_TC89_L1,cv2.CHAIN_APPROX_TC89_KCOS]
      typeno = int(data['method'])
      if typeno < 0 or typeno >= len(types):
        typeno = 0
      threshold_type = types[typeno]

    def func(frame, opts={}):
      ret,f = cv2.threshold(frame, value, mx, threshold_type if threshold_type else cv2.CHAIN_APPROX_NONE)
      return f
    return enhance(func)


  if typ == 'dilate':
    val = data['kernel'] if 'kernel' in data else 7
    iters = data['iterations'] if 'iterations' in data else 3
    kernel = np.ones((val, val),np.uint8)

    def func(frame, opts={}):
      return cv2.dilate(frame, kernel, iterations=iters)

    return enhance(func)

  if typ == 'contours':     
    mode = select(data['mode'] if 'mode' in data else 0, [cv2.RETR_EXTERNAL, cv2.RETR_LIST, cv2.RETR_CCOMP, cv2.RETR_TREE, cv2.RETR_FLOODFILL], aliases=['RETR_EXTERNAL', 'RETR_LIST', 'RETR_CCOMP', 'RETR_TREE', 'RETR_FLOODFILL'])
    method = select(data['method'] if 'method' in data else 0, [cv2.CHAIN_APPROX_NONE,cv2.CHAIN_APPROX_SIMPLE,cv2.CHAIN_APPROX_TC89_L1,cv2.CHAIN_APPROX_TC89_KCOS])
    drawlines = drawboxes = data['drawlines'] if 'drawlines' in data else True
    linethickness = select(data['linethickness'] if 'linethickness' in data else 1, [-1,0,1,2,3,4,5], aliases=[-1,0,1,2,3,4,5])
    drawboxes = data['drawboxes'] if 'drawboxes' in data else False
    minsize = select_int(data['minsize'] if 'minsize' in data else 0, max=4000)

    def func(frame, opts={}):
      contours,hierarchy = cv2.findContours(frame, mode, method)
      if drawlines:
        frame = cv2.drawContours(frame,contours,-1,(255,255,0),linethickness)
      if drawboxes:
        for c in contours:
          bx,by,bw,bh = cv2.boundingRect(c)
          if minsize == 0 or (bw*bh) >= minsize:
            frame = cv2.rectangle(frame,(bx,by),(bx+bw,by+bh),(255,255,0),linethickness)
      return frame
    return enhance(func)


  if typ == 'canny':
    threshold1 = select_int(data['threshold1'] if 'threshold1' in data else 0, max=500)
    threshold2 = select_int(data['threshold2'] if 'threshold2' in data else 82, max=500)
    def func(frame, opts={}):
      return cv2.Canny(frame, threshold1, threshold2)
    return enhance(func)

  if typ == 'add':
    factor = select_float(data['factor'] if 'factor' in data else 1.0, max=10.0)
    def func(f, opts={}):
      if not 'base' in opts: return f
      return opts['base'] + f * factor

    return enhance(func)

  if typ == 'bgsub':
    algo = select(data['algo'] if 'algo' in data else 'MOG2', ['MOG2', 'KNN'])
    showmask = data['showmask'] if 'showmask' in data else False
    backSub =  cv2.createBackgroundSubtractorKNN() if algo == 'KNN' else  cv2.createBackgroundSubtractorMOG2()
    def func(f, opts={}):
      fgMask = backSub.apply(f)
      return fgMask if showmask else f*fgMask

    return enhance(func)

def create_processor_from_data(data):
  '''
  Takes parsed (json) data which should a contain a 'processors' key with an ordered list/array of processor configurations.
  It returns a processor, which is basically a method that takes a frame and returns a new (processed) frame. This processors,
  runs a list of sub-processors, each instantiated from one item in the list of processor configurations.
  '''

  # create processor for each processor in config
  processors = []
  for processor_data in data['processors']:
    p = create_processor(processor_data)
    if p:
      processors.append(p)

  # create wrapping processor which runs individual processors in sequence
  return _create_wrapping_processor(processors)

def create_processor_from_json_file(filepath):
  '''
  Tries to load json data form the specified filepath.
  Then uses the create_processor_from_data method to create the processor and returns it to the caller.
  If any of these steps fail, None is returned.
  '''
  data = _get_data_from_json_file(filepath)
  return None if data == None else create_processor_from_data(data)


class FuncWrapper:
  def __init__(self, func):
    self.func = func

  def run(self, frame, opts={}):
    return self.func(frame, opts) if self.func else frame

  __call__ = run

def create_controlled_processor(winid, idx, data):
  original_processor = create_processor(data)
  processor = FuncWrapper(original_processor)

  def update_processor():
    processor.func = create_processor(data)

  typ = data['type'] if 'type' in data else None

  def ctrl(param, max=None, initialValue=None, valueProc=None, values=None, factor=None, readProc=None, default=None):
    addParamTrackbar(winid, data, param,
      max=max,
      initialValue=initialValue,
      valueProc=valueProc,
      values=values,
      factor=factor,
      readProc=readProc,
      onChange=update_processor,
      controlId='{}_{}-{}'.format(idx, typ, param),
      default=default)
  
  # print('create_controlled_processor: {}'.format(data))

  ctrl('enabled', values=[False, True], default=True)
  ctrl('verbose', values=[False, True], default=False)

  typ = data['type'] if 'type' in data else None

  if typ == 'reframe':
    ctrl('factor', factor=2000)

  if typ == 'gaussianblur':
    ctrl('y', values=[0,1,3,5,7,9,11,13,15,17,19])
    ctrl('x', values=[0,1,3,5,7,9,11,13,15,17,19])
    ctrl('sigma-x', 10)
    ctrl('sigma-y', 10)

  if typ == 'threshold':
    ctrl('value', 255)
    ctrl('max', 255)
    ctrl('method', values=[cv2.THRESH_BINARY,cv2.THRESH_BINARY_INV,cv2.THRESH_TRUNC,cv2.THRESH_TOZERO,cv2.THRESH_TOZERO_INV,cv2.THRESH_MASK])

  if typ == 'dilate':
    ctrl('kernel', 20)
    ctrl('iterations', 10)

  if typ == 'contours':
    ctrl('mode', values=[cv2.RETR_EXTERNAL, cv2.RETR_LIST, cv2.RETR_CCOMP, cv2.RETR_TREE, cv2.RETR_FLOODFILL])
    ctrl('method', values=[cv2.CHAIN_APPROX_NONE,cv2.CHAIN_APPROX_SIMPLE,cv2.CHAIN_APPROX_TC89_L1,cv2.CHAIN_APPROX_TC89_KCOS])
    ctrl('drawlines', values=[False,True])
    ctrl('drawboxes', values=[False,True])
    ctrl('linethickness', values=[-1,0,1,2,3,4,5])
    ctrl('minsize', 4000)

  if typ == 'canny':
    ctrl('threshold1', 500)
    ctrl('threshold2', 500)

  if typ == 'blur':
    ctrl('x', 5)
    ctrl('y', 5)

  if typ == 'lerp-result':
    ctrl('factor', 2000)

  if typ=='add':
    ctrl('factor', max=1000, initialValue=1, valueProc=lambda x: (x/100.0)-100.0)

  if typ=='bgsub':
    ctrl('algo', values=['MOG2', 'KNN'])
    ctrl('showmask', values=[False,True])

  #   addParamTrackbar(winid, params, 'blur-x', values=[0,1,3,5,7,9,11,13,15,17,19])
  #   addParamTrackbar(winid, params, 'blur-y', values=[0,1,3,5,7,9,11,13,15,17,19])
  #   addParamTrackbar(winid, params, 'blur-sigma-x', 10)
  #   addParamTrackbar(winid, params, 'blur-sigma-y', 10)
  return processor

def create_controlled_processor_from_data(data, winid='Processors'):
    # create window    
    cv2.namedWindow(winid, cv2.WINDOW_NORMAL)
    # cv2.moveWindow(winid, 5, 5 + 400*idx)
    # cv2.resizeWindow(winid, 500,400)
    # cv2.setWindowProperty(winid,cv2.WND_PROP_FULLSCREEN,cv2.WINDOW_FULLSCREEN)
    cv2.setWindowProperty(winid,cv2.WND_PROP_FULLSCREEN,cv2.WINDOW_NORMAL)

    # create controls and processors
    processors = []
    items = data['processors'] if 'processors' in data else []
    for idx, processor_data in enumerate(items):
      processors.append(create_controlled_processor(winid, idx, processor_data))

    return _create_wrapping_processor(processors)

def create_controlled_processor_from_json_file(filepath, winid='Processors'):
  '''
  Tries to load json data form the specified filepath.
  Then uses the create_processor_from_data method to create the processor and returns it to the caller.
  If any of these steps fail, None is returned.
  '''
  data = _get_data_from_json_file(filepath)
  return None if data == None else create_controlled_processor_from_data(data, winid=winid)

  