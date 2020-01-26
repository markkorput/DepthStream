#include "catch.hpp"

#include <iostream>
#include <memory>
#include "DepthStream/Frame.h"

using namespace depth;

TEST_CASE("depth::[ReadOnly]Frame", ""){
  SECTION("default ctor"){
    Frame f;
    REQUIRE(f.data() == NULL);
    REQUIRE(f.size() == 0);
  }

  SECTION("allocating ctor"){
    Frame f(16);
    REQUIRE(f.data() != NULL);
    REQUIRE(f.size() == 16);
  }

  SECTION("referncing ctor"){
    char buf[32];

    {
      Frame f(buf, 32);
      REQUIRE(f.data() == buf);
      REQUIRE(f.size() == 32);
    }

    // write to buf to check it's not deallocated
    memset(buf, 0, 32);
  }

  SECTION("convert") {
    // TODO
  }
}

TEST_CASE("depth::WritableFrame", ""){
  SECTION("write") {
    WritableFrame f(8);
    f.write("abc", 3);
    REQUIRE(((char*)f.data())[0] == 'a');
    REQUIRE(((char*)f.data())[1] == 'b');
    REQUIRE(((char*)f.data())[2] == 'c');
  }

  SECTION("concat") {
    std::string v1 = "abc";
    Frame f1(v1.c_str(), 3);

    std::string v2 = "xyz";
    Frame f2(v2.c_str(), 3);

    WritableFrame f(0);
    REQUIRE(f.size() == 0);
    f.concat(f1, f2);
    REQUIRE(f.size() == 6);

    std::string endval = v1+v2;
    REQUIRE(memcmp(f.data(), endval.c_str(), 6)== 0);
  }
}