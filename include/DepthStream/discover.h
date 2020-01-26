#pragma once

#include <stdlib.h>
#include <memory>
#include <vector>
#include <string>

namespace discover {

  class Service;
  typedef std::shared_ptr<Service> ServiceRef;

  class Service {
    public:
      virtual void start(){}
      virtual void stop(){}
      
      virtual void addUdpConsumer(const std::string& host, int port) {}
  };



  class ServiceProvider;
  typedef std::shared_ptr<ServiceProvider> ServiceProviderRef;

  class ServiceProvider {
    public:
      static ServiceProviderRef create(const std::string& serviceTypeId, ServiceRef service=nullptr);

    public:

      ServiceProvider(const std::string& serviceTypeId, ServiceRef service) 
        : serviceTypeId(serviceTypeId), serviceRef(service){
          this->start();
        }

      void start();
      void stop();

    protected:
      void onConnectRequest(std::string host, int port);

    private:

      int mPort = 4445;
      std::string serviceTypeId;
      ServiceRef serviceRef = nullptr;

      void* serverThread = NULL;
  };



  class OscFrameService;
  typedef std::shared_ptr<OscFrameService> OscFrameServiceRef;

  class OscFrameService : public Service {
    public:

    typedef struct {
      std::string host;
      std::string port;
    } ConsumerInfo;

    public:
      static OscFrameServiceRef create() {
        return std::make_shared<OscFrameService>();
      }

    public:

      OscFrameService() {
        this->start();
      }
    
      void submit(const void* data, size_t size);
      void addUdpConsumer(const std::string& host, int port) override;

    private:

      std::string addr = "/depthframe";
      std::vector<ConsumerInfo> consumers;
  };


}
