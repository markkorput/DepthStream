#include "catch.hpp"

#include <iostream>
#include <memory>
#include "DepthStream/Buffer.h"

using namespace depth;

TEST_CASE("depth::Buffer", ""){
  SECTION("setOutput"){
    const size_t size = 64;
    char data[size];
    depth::Buffer buffer;

    size_t count = 0;
    buffer.setOutput([&count](const void* d, size_t s){
      count += s;
    });

    REQUIRE(count == 0);
    REQUIRE(buffer.getRef() == nullptr);
    buffer.write(data, 32);
    REQUIRE(count == 32);
    REQUIRE(buffer.getRef()->data() == data);
    buffer.write(data, 32);
    REQUIRE(count == 64);
    REQUIRE(buffer.getRef()->data() == data);
  }
}