#pragma once

#include <stdlib.h>
#include <memory>
#include <string>
#include <chrono>
#include <math.h>

#include "PacketSender.h"
#include "ServiceConnectionListener.h"

namespace discover { namespace osc { namespace service {

  class PacketService {
    public:
      PacketService(std::string serviceId, int port, bool start=true) : mServiceId(serviceId), mPort(port) {
        mUpdateTime = getTime();
        if (start)
          this->start();
      }

      void start();
      void stop();

      inline void update(){ this->update(getTime() - mUpdateTime); }
      inline void update(float dt){ this->update(floor(dt*1000.0f)); }
      void update(uint32_t dtMs);

      void submit(const void* data, size_t size);

    protected:

      inline uint32_t getTime() {
        return (uint32_t)std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::steady_clock::now().time_since_epoch()).count();
      }

    private:
      std::string mServiceId;
      int mPort;
      ServiceConnectionListenerRef serviceConnectionListenerRef = nullptr;
      PacketSenderRef packetSenderRef = nullptr;

      // broadcast
      uint32_t mUpdateTime, mNextBroadcastTime;
      uint32_t mBroadcastIntervalMs = 1000;
  };

 } } } // namespace discover::osc::service

