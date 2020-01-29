#pragma once

#include <stdlib.h>
#include <string>
#include <vector>
#include <thread>
#include <memory>

namespace discover { namespace socket {

  typedef std::function<void(int)> ConsumerInfoCallback;

  namespace server {
    typedef struct {
      
      std::thread* thread;
      int socket = 0;
      bool active=true;
    } ServerInfo;

    typedef std::shared_ptr<ServerInfo> Handle;

    Handle create(int mPort, ConsumerInfoCallback callback=nullptr);
  }


  namespace connect {

    typedef struct {
      std::string id;
      int socket;
      bool connected=true;
    } ConsumerInfo;

    
    void add_connect_request_callback(server::Handle server, const std::string& serviceId, ConsumerInfoCallback callback);

    inline void add_consumer(std::vector<ConsumerInfo>& consumers, int socket) {
      ConsumerInfo i;
      i.socket = socket;
      consumers.push_back(i);
    }
  }

 

  namespace packet {
    void send(std::vector<connect::ConsumerInfo>& consumers, const void* data, size_t size);
  }
}}