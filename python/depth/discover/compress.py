
import zlib

def compress(buffer, size):
  return None

def decompress(buffer, size):
  view = memoryview(buffer)[0:size]
  decompressed_data = zlib.decompress(view)
  return decompressed_data