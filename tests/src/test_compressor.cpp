#include "catch.hpp"

#include <iostream>
#include <memory>
#include "DepthStream/compression.h"
#include "DepthStream/Compressor.h"

using namespace depth;

TEST_CASE("depth::Compressor", ""){
  SECTION("compress"){

    // original data
    size_t size=512;
    char data[size];
    for (int i=0; i<size; i++) data[i]=i;

    // compression buffer
    size_t max=1024;
    char compress_buf[max];

    // compress
    Compressor compressor;

    REQUIRE(compressor.compress(data, size));
    REQUIRE(compressor.getSize() < size);

    void* inflate_buf = NULL;
    size_t inflate_bufsize = 0;
    size_t inflate_size = compression::inflate(compressor.getData(), compressor.getSize(), inflate_buf, inflate_bufsize);

    REQUIRE(inflate_size == size);
    REQUIRE(memcmp(inflate_buf, data, inflate_size) == 0);
    
    // cleanup
    compression::freeBuffer(inflate_buf);
  }
}