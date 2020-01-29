
import zlib

def compress(buffer, size):
  return None

def decompress(buffer, size):
  view = memoryview(buffer)[0:size]
  decompressed_data = None
  try:
    decompressed_data = zlib.decompress(view)
  except zlib.error:
    decompressed_data = None

  return decompressed_data