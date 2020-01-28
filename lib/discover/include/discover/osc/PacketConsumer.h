#pragma once

#include <stdlib.h>
#include <memory>
#include <string>
#include <chrono>
#include <math.h>

#include "osc.h"
#include <discover/middleware.h>

namespace discover { namespace osc {

  class PacketConsumer;
  typedef std::shared_ptr<PacketConsumer> PacketConsumerRef;

  class PacketConsumer {

    public: // types

      typedef std::function<void(const void*, size_t)> DataHandler;

    public: // static factory methods

      static PacketConsumerRef create(const std::string& serviceId, DataHandler func, bool start=true) {
        auto ref = std::make_shared<PacketConsumer>(serviceId, false);
        
        ref->dataCallback = func;

        if (start)
          ref->start();

        return ref;
      }

    public: // methods

      PacketConsumer(const std::string& serviceId, bool start=true) : mServiceId(serviceId) {
        if (start) {
          this->start();
        }
      }

      void start();
      void stop();

    protected:

      inline void onData(const void* data, size_t size) {
        if (this->dataCallback)
          this->dataCallback(data, size);
      }

    private:

      std::string mServiceId;
      std::string messageAddr = "/frame";
      int mPort=4445; // default UDP port on which to receive data

      DataHandler dataCallback = nullptr;
      server::InstanceHandle dataServerHandle = NULL;
  };
}}
