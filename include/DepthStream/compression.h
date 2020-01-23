#pragma once

#include <stdlib.h>


namespace depth { namespace compression {
  // buffer-space management
  const size_t DEFAULT_GROW_SIZE = 1024;
  void* growBuffer(void* buffer, size_t currentsize, size_t newsize, bool freeOldBuffer=true);
  void freeBuffer(void* buffer);

  // compress
  size_t deflate(const void* data, size_t size, void* out, size_t out_size);

  // decompress
  size_t inflate(const void* compressedData, size_t compressedSize, void* &target, size_t &target_size, bool growTarget=true, size_t grow_size=DEFAULT_GROW_SIZE, bool bVerbose=false); 
}}