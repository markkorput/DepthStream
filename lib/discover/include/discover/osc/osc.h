#pragma once
#include <stdlib.h>
#include <string>
#include <vector>


namespace discover { namespace osc {
  /**
   * Broadcasts OSC message(s) announcing the presence and information of the service.
   * 
   * @param serviceId a string-based service identifier, that will be post-fixed to the 
   * address field of the broadcasted OSC message(s).
   * @param ports The (UDP) ports at which the broadcasted messages will be sent
   * @param url The url of the service (ie. "osc.udp://hostname.local:4445/")
   */
  void broadcast_service(const std::string& serviceId, const std::vector<int>& ports, const std::string& url);

  /**
   * Uses broadcast_service(const std::string& serviceId, std::vector<int> ports, const std::string& url)
   * method to broadcast the service announcement at the default ports: 4445, 4446 and 4447
   */
  inline void broadcast_service(const std::string& serviceId, const std::string& url) {
    broadcast_service(serviceId, std::vector<int>{ 4445, 4446, 4447 }, url);
  }

  namespace ServiceConnectionListener {
    /**
     * Dummy class for readability; note that we're using void pointers, instead of a
     * lo::ServerThread pointers, to limit the dependency on the liblo library to osc.cpp
     */
    typedef void Instance;
    typedef std::function<void(const std::string& host, int port)> ConsumerInfoCallback;

    Instance* start(const std::string& serviceId, int port, ConsumerInfoCallback callback, int maxPortAttempts=10);
    bool stop(Instance* instance);
    const std::string get_url(Instance* instance);
  }
}}