#pragma once

#include <stdlib.h>
#include <string>
#include <vector>


namespace discover { namespace osc {

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

    typedef std::function<void(const void*, size_t)> DataFunc;

    void add_packet_callback(server::InstanceHandle serverHandle, DataFunc callback);
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

    void add_service_found_callback(server::InstanceHandle server, const std::string& serviceId, ServiceInfoFunc callback);
  }

  namespace connect {
    void sendConsumerConnectRequest(const std::string& serviceId, const std::string& serviceHost, int servicePort, const std::string& consumerUrl);
  }

  namespace ServiceConnectionListener {
    /**
     * Dummy class for readability; note that we're using void pointers, instead of a
     * lo::ServerThread pointers, to limit the dependency on the liblo library to osc.cpp
     */
    typedef server::Instance Instance;
    typedef std::function<void(const std::string& host, int port)> ConsumerInfoCallback;

    Instance* start(const std::string& serviceId, int port, ConsumerInfoCallback callback, int maxPortAttempts=10);
    inline bool stop(Instance* instance) { return server::destroy(instance); }
    inline const std::string get_url(Instance* instance) { return server::get_url(instance); }
  }

  typedef struct {
    std::string host;
    std::string port;
  } ConsumerInfo;

  inline void add_consumer(std::vector<ConsumerInfo> consumers, const std::string& host, int port) {
    ConsumerInfo i;
    i.host = host;
    i.port = std::to_string(port);
    consumers.push_back(i);
  }

  void sendPacket(const std::vector<ConsumerInfo>& consumers, const void* data, size_t size, const std::string& messageAddr);

  inline void sendPacket(const std::vector<ConsumerInfo>& consumers, const void* data, size_t size) {
    sendPacket(consumers, data, size, "/data");
  }

    
}}