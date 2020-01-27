#pragma once

#include <stdlib.h>
#include <memory>
#include <string>

#include "PacketSender.h"
#include "ServiceConnectionListener.h"

namespace discover { namespace osc { namespace service {

  class PacketService {
    public:
      PacketService(std::string serviceId, int port) : mServiceId(serviceId), mPort(port) {
        this->start();
      }

      void submit(const void* data, size_t size);
      void start();
      void stop();

    private:
      std::string mServiceId;
      int mPort;
      ServiceConnectionListenerRef serviceConnectionListenerRef = nullptr;
      PacketSenderRef packetSenderRef = nullptr;
  };

 } } } // namespace discover::osc::service

