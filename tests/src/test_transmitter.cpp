#include "catch.hpp"

#include <iostream>
#include <memory>
#include "DepthStream/Transmitter.h"

using namespace depth;

TEST_CASE("depth::Transmitter", ""){
  SECTION("(default) Transmitter"){
    auto transmitterRef = OscTransmitter::create();
  }
}