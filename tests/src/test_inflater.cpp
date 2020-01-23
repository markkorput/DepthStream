#include "catch.hpp"

#include <iostream>
#include <memory>
#include "DepthStream/compression.h"
#include "DepthStream/Inflater.h"

using namespace depth;

TEST_CASE("depth::Inflater", ""){
  SECTION("inflate"){

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

    // inflate
    Inflater inflater(1024);
    REQUIRE(inflater.inflate(compress_buf, compress_size));
    REQUIRE(inflater.getSize() == size);
    REQUIRE(memcmp(inflater.getData(), data, inflater.getSize()) == 0);

    compression::freeBuffer(compress_buf);
  }
}