#include <discover/middleware.h>
#include <chrono>
#include <math.h>

uint32_t getTimeMs() {
  return (uint32_t)std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::steady_clock::now().time_since_epoch()).count();
}

using namespace discover::middleware;

PacketMiddlewareFunc discover::middleware::throttle_max_fps(float fps) {

  struct {
    uint32_t nextTimeMs;
    uint32_t intervalMs;
  } timer = { getTimeMs(), (uint32_t) floor(1000.0f / fps) };

  return [timer](Packet& p) -> Packet* {
    static uint32_t next = timer.nextTimeMs;
    static uint32_t interval = timer.intervalMs;

    auto t = getTimeMs();
    if (t < next) return NULL;
    next = t + interval;
    return &p;
  };  
}

PacketMiddlewareFunc discover::middleware::compress() {
  // return [](Packet& p) {
  //   return &p;
  // };  
  return nullptr;
}
