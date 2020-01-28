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
        // todo add data handler
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

    private:

      std::string mServiceId;
      std::string messageAddr = "/frame";
      int mPort=4445; // default UDP port on which to receive data

      server::InstanceHandle dataReceiverHandle = NULL;
  };


}}
