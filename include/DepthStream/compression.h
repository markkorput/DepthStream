#pragma once

#include <stdlib.h>
#include <discover/middleware.h>

namespace depth { namespace compression {
  
  // buffer-space management
  const size_t DEFAULT_GROW_SIZE = 1024;
  void* growBuffer(void* buffer, size_t currentsize, size_t newsize, bool freeOldBuffer=true);
  void freeBuffer(void* buffer);

  class CompressBuffer {
    public:
      void* data;
      size_t size;

      ~CompressBuffer() {
        if (data) freeBuffer(data);
      }
  };

  // compress
  size_t deflate(
    const void* data, size_t size,
    void* &out, size_t &out_size,
    bool growTarget=true,                               // allow target buffer re-allocation
    size_t grow_size=DEFAULT_GROW_SIZE,                 // amount by which to increase target buffer size
    bool bVerbose=false);                               // verbosity flag

  // decompress
  size_t inflate(
    const void* compressedData, size_t compressedSize,  // source data
    void* &target, size_t &target_size,                 // target buffer
    bool growTarget=true,                               // allow target buffer re-allocation
    size_t grow_size=DEFAULT_GROW_SIZE,                 // amount by which to increase target buffer size
    bool bVerbose=false);                               // verbosity flag

  namespace middleware {
    discover::middleware::packet::ConvertFunc compress(CompressBuffer& buffer);
    discover::middleware::packet::ConvertFunc compress();
  }
}}