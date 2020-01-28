#pragma once

#include <functional>

namespace discover { namespace middleware {

  typedef struct {
    const void* data;
    size_t size;
  } Packet;

  typedef std::function<Packet*(Packet &packet)> PacketMiddlewareFunc;

  PacketMiddlewareFunc throttle_max_fps(float fps);
  PacketMiddlewareFunc compress();
}}