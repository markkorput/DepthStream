#include "catch.hpp"

#include <iostream>
#include <memory>
#include "DepthStream/compression.h"

using namespace depth;

TEST_CASE("depth::compression", ""){
  SECTION("deflate and inflate"){
    // original data
    size_t size=512;
    char data[size];
    for (int i=0; i<size; i++) data[i]=i;

    // compression buffer
    size_t max=0;
    void* compress_buf=NULL;

    // compress
    size_t compress_size = compression::deflate(data, size, compress_buf, max);
    REQUIRE(compress_size < size);

    { // inflate buffer
      void* inflatebuf = NULL;
      size_t inflatebufsize = 0;

      // inflate, dynamically growing target buffer
      size_t inflatesize = compression::inflate(compress_buf, compress_size, inflatebuf, inflatebufsize, true /* grow target buffer when necessary */, compression::DEFAULT_GROW_SIZE, true /* verbose */);
      REQUIRE(inflatesize == size);
      REQUIRE(inflatebuf != NULL);

      // verify data integrity
      REQUIRE(inflatebufsize > inflatesize);
      REQUIRE(memcmp(inflatebuf, (void*)data, size) == 0);
    
      // cleanup
      compression::freeBuffer(inflatebuf);
      inflatebuf = NULL;
      inflatebufsize = 0;
    }

    { // inflate with (too small) fixed buffer, don't allow resize
      size_t inflatebufsize = 16;
      char inflatebuf[inflatebufsize];
      void* pbuf = (void*)inflatebuf;

      size_t inflatesize = compression::inflate(compress_buf, compress_size, pbuf, inflatebufsize, false /* don't grow buffer */);
      REQUIRE(pbuf == (void*)inflatebuf);
      REQUIRE(inflatesize == 0); //failure, didn't fit in buffer
    }

    { // inflate with (large enough small) fixed buffer, don't allow resize
      size_t inflatebufsize = 512;
      char inflatebuf[inflatebufsize];
      void* pbuf = (void*)inflatebuf;

      size_t inflatesize = compression::inflate(compress_buf, compress_size, pbuf, inflatebufsize, false /* don't grow buffer */);
      REQUIRE(inflatesize == size); //failure, didn't fit in buffer
      REQUIRE(pbuf == (void*)inflatebuf); // success
    }

    compression::freeBuffer(compress_buf);
  }
}