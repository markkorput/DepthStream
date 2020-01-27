#pragma once

#include <stdlib.h>
#include <memory>
#include <vector>
#include <string>

namespace discover { namespace osc {

  namespace service {
    class ServiceConnectionListener;
    typedef std::shared_ptr<ServiceConnectionListener> ServiceConnectionListenerRef;

    class ServiceConnectionListener {
      public:
        typedef std::function<void(const std::string& host, int port)> UdpRemoteFunc;
      
      public:
        static ServiceConnectionListenerRef create(const std::string& serviceTypeId, int port, UdpRemoteFunc udpRemoteFunc);

      public:

        ServiceConnectionListener(const std::string& serviceTypeId, int port, UdpRemoteFunc func) 
          : serviceTypeId(serviceTypeId), mPort(port), mUdpRemoteFunc(func){
            this->start();
          }

        void start();
        void stop();

      protected:
        void onConnectRequest(std::string host, int port);

      private:

        int mPort = 4445;
        std::string serviceTypeId;
        UdpRemoteFunc mUdpRemoteFunc = nullptr;

        void* serverThread = NULL;
    };


    class PacketSender;
    typedef std::shared_ptr<PacketSender> PacketSenderRef;

    class PacketSender {

      typedef struct {
        std::string host;
        std::string port;
      } ConsumerInfo;

      public:
        static PacketSenderRef create() {
          return std::make_shared<PacketSender>();
        }

      public:

        // PacketSender() {}

        void submit(const void* data, size_t size);
        void addUdpConsumer(const std::string& host, int port);

      private:

        std::string addr = "/depthframe";
        std::vector<ConsumerInfo> consumers;
    };



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
  }
 } } // namespace discover namespace osc

