#pragma once

#include <stdlib.h>
#include <memory>
#include <string>

namespace discover { namespace osc { namespace service {

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

      const std::string& getUrl() { return mUrl; }

    protected:
      void onConnectRequest(std::string host, int port);

    private:

      int mPort = 4445;
      std::string serviceTypeId;
      UdpRemoteFunc mUdpRemoteFunc = nullptr;
      std::string mUrl = "";

      void* serverThread = NULL;
  };

 } } } // namespace discover::osc::service

