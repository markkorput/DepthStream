#include "catch.hpp"

#include <iostream>
#include <memory>
#include "DepthStream/Compressor.h"

using namespace depth;

TEST_CASE("depth::Compressor", ""){
  SECTION("static deflate method"){
    
    size_t size=256, max=512;
    char buf[max];
    char data[size];
    for (int i=0; i<size; i++) buf[i]=i;

    size_t newsize = Compressor::deflate(data, size, buf, max);
    REQUIRE(newsize < size);

    // TODO; inflate and verify data is the same
  }
}