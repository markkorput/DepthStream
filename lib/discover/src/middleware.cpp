#include <discover/middleware.h>
#include <chrono>
#include <iostream>
#include <math.h>

using namespace std;

uint32_t getTimeMs() {
  return (uint32_t)std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::steady_clock::now().time_since_epoch()).count();
}

using namespace discover::middleware::packet;

ConvertFunc discover::middleware::packet::throttle_max_fps(float fps) {

  struct {
    uint32_t nextTimeMs;
    uint32_t intervalMs;
  } timer = { getTimeMs(), (uint32_t) floor(1000.0f / fps) };

  return [timer](Packet& p) -> Packet* {
    // cout << "throttle " << p.size << " bytes" << endl;
    static uint32_t next = timer.nextTimeMs;
    static uint32_t interval = timer.intervalMs;

    auto t = getTimeMs();
    if (t < next) return NULL;
    next = t + interval;
    return &p;
  };  
}
