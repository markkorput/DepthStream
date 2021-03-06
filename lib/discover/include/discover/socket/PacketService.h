#pragma once

#include <stdlib.h>
#include <memory>
#include <string>
#include <chrono>
#include <math.h>

#include <discover/socket.h>

namespace discover { namespace socket {

  class PacketService {
    public:

      PacketService(std::string serviceId, int port, bool start=true) : mServiceId(serviceId), mPort(port) {
        mUpdateTime = mNextBroadcastTime = getTime();
        
        if (start)
          this->start();
      }

      ~PacketService() { this->stop(); }

      /**
       * Starts listeing for incoming connection requests
       * and resets the internal broadcast interval timer.
       */
      void start();
      void stop();

      /**
       * Convenience method for callers who don't want to handle time-management.
       * This method calls this->update(uint32_t) overload method, with the number
       * of milliseconds that passed since the last call to this->update(uint32_t).
       */
      inline void update(){ this->update(getTime() - mUpdateTime); }

      /**
       * Convenience method for callers who handle time-management in seconds.
       * This method convert seconds to milliseconds and call this->update(uint32_t).
       */
      inline void update(float dt){ this->update(floor(dt*1000.0f)); }

      /**
       * Updates the internal broadcast interval timer
       */
      void update(uint32_t dtMs);

      /**
       * Uses the internal PacketSender instance (if initialized by a call
       * to this service's stat method.) to send out the data to connected consumers.
       */
      inline void submit(const void* data, size_t size) {
        socket::packet::send(mConsumers, data, size);
      }

    protected:
        
      /**
       * System time fetcher, used for timing the internal broadcast interval.
       */
      inline uint32_t getTime() {
        return (uint32_t)std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::steady_clock::now().time_since_epoch()).count();
      }

    private:
      std::string mServiceId;
      int mPort;
      server::Handle connectionListener = 0;
      // std::string mConnectionListenerUrl;
      std::vector<connect::ConsumerInfo> mConsumers;

      // broadcast timer
      uint32_t mUpdateTime, mNextBroadcastTime;
      uint32_t mBroadcastIntervalMs = 1000;
  };    
} } // namespace discover::socket

