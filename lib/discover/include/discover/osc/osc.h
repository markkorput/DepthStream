#pragma once

#include <stdlib.h>
#include <string>
#include <vector>


namespace discover { namespace osc {

  struct urlinfo {
    std::string protocol, host, port, path, query;
    urlinfo(const std::string& url_s);
  };

  namespace server {
    typedef void Instance;
    typedef void* InstanceHandle;

    InstanceHandle create(int port, int maxPortAttempts, bool start=false);
    inline InstanceHandle create(int port, bool start=false) {
      return create(port, 25, start);
    }

    void start(InstanceHandle instance);
    bool destroy(InstanceHandle instance);
    const std::string get_url(InstanceHandle instance);
  }

  namespace broadcast {
    const std::vector<int> DEFAULT_PORTS{ 4445, 4446 };
    typedef std::function<void(std::string, int)> ServiceInfoFunc;

    /**
     * Broadcasts OSC message(s) announcing the presence and information of the service.
     * 
     * @param serviceId a string-based service identifier, that will be post-fixed to the 
     * address field of the broadcasted OSC message(s).
     * @param ports The (UDP) ports at which the broadcasted messages will be sent
     * @param url The url of the service (ie. "osc.udp://hostname.local:4445/")
     */
    void announce(const std::string& serviceId, const std::vector<int>& ports, const std::string& url);

    /**
     * Uses announce(const std::string& serviceId, std::vector<int> ports, const std::string& url)
     * method to broadcast the service announcement at the default ports: 4445, 4446 and 4447
     */
    inline void announce(const std::string& serviceId, const std::string& url) {
      announce(serviceId, DEFAULT_PORTS, url);
    }

    void addServiceFoundCallback(server::InstanceHandle server, const std::string& serviceId, ServiceInfoFunc callback);
  }

  namespace connect {
    typedef std::function<void(std::string, int)> ConsumerInfoCallback;

    typedef struct {
      std::string host;
      std::string port;
    } ConsumerInfo;

    inline void addConsumer(std::vector<ConsumerInfo>& consumers, const std::string& host, int port) {
      ConsumerInfo i;
      i.host = host;
      i.port = std::to_string(port);
      consumers.push_back(i);
    }

    void sendConsumerConnectRequest(const std::string& serviceId, const std::string& serviceHost, int servicePort, const std::string& consumerUrl);
    void addConnectRequestCallback(server::InstanceHandle server, const std::string& serviceId, ConsumerInfoCallback callback);
  }

  namespace packet {
    typedef std::function<void(const void*, size_t)> DataFunc;
    const std::string DEFAULT_MESSAGE = "/data";
    void* growBuffer(void* buffer, size_t currentsize, size_t newsize, bool freeOldBuffer);
    void freeBuffer(void* buffer);

    class Buffer {
      public:
        void* data = NULL;
        size_t size = 0;

        ~Buffer() {
          if (data) freeBuffer(data);
        }
    };

    void addCallback(server::InstanceHandle serverHandle, DataFunc callback, const std::string& messageAddr, Buffer* buffer);
    
    inline void addCallback(server::InstanceHandle serverHandle, DataFunc callback) {
      addCallback(serverHandle, callback, DEFAULT_MESSAGE, NULL);
    }

    inline void addCallback(server::InstanceHandle serverHandle, DataFunc callback, Buffer& buffer) {
      addCallback(serverHandle, callback, DEFAULT_MESSAGE, &buffer);
    }

    void send(const std::vector<connect::ConsumerInfo>& consumers, const void* data, size_t size, const std::string& messageAddr);

    inline void send(const std::vector<connect::ConsumerInfo>& consumers, const void* data, size_t size) {
      send(consumers, data, size, DEFAULT_MESSAGE);
    }
  }

}}