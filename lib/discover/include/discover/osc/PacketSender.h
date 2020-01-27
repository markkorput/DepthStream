#pragma once

#include <stdlib.h>
#include <memory>
#include <vector>
#include <string>

namespace discover { namespace osc { namespace service {

  class PacketSender;
  typedef std::shared_ptr<PacketSender> PacketSenderRef;

  class PacketSender {

    protected:

      typedef struct {
        std::string host;
        std::string port;
      } ConsumerInfo;

    public:

      static PacketSenderRef create() {
        return std::make_shared<PacketSender>();
      }

    public:

      void submit(const void* data, size_t size);
      void addUdpConsumer(const std::string& host, int port);

    private:

      std::string addr = "/depthframe";
      std::vector<ConsumerInfo> consumers;
  };


 } } } // namespace discover::osc::service
