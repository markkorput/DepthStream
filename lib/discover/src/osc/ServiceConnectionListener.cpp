#include <cstdio>
#include <iostream>
#include <functional>
#include <lo/lo.h>
#include <lo/lo_cpp.h>

#include "discover/osc/ServiceConnectionListener.h"

using namespace std;
using namespace discover;
using namespace discover::osc;
using namespace discover::osc::service;

ServiceConnectionListenerRef ServiceConnectionListener::create(const string& serviceTypeId, int port, UdpRemoteFunc udpRemoteFunc) {
  return make_shared<ServiceConnectionListener>(serviceTypeId, port, udpRemoteFunc);
}

void ServiceConnectionListener::start() {
  int attempt = 0;
  lo::ServerThread* st;
  int port = mPort;

  do {
    printf("Attempting to start OSC server on port: %i\n", port);
    st = new lo::ServerThread(port);
    port += 1;
  } while (!st->is_valid() && port < (mPort+10));

  if (!st->is_valid()) {
      std::cout << "Failed to create valid lo::ServerThread. (ports already in use by other services)" << std::endl;
      return;
  }

  mPort += port-1;

  // st->set_callbacks([&st](){
  //     printf("Thread init: %p.\n",&st);},
  //     [](){printf("Thread cleanup.\n");});

  std::cout << "URL: " << st->url() << std::endl;
  this->mUrl = st->url();

  string addr ="/discover/connect/"+this->serviceTypeId;
  printf("registering OSC handler for: %s\n", addr.c_str());

  st->add_method(addr, "si", [this](lo_arg **argv, int) {
    string host = &argv[0]->s;
    int port = argv[1]->i;
    this->onConnectRequest(host, port);
  });

  st->start();

  this->serverThread = (void*) st;
}

void ServiceConnectionListener::stop() {
  if (serverThread) {
    lo_server_thread_free((lo::ServerThread*)serverThread);
    serverThread = NULL;
  }
}

void ServiceConnectionListener::onConnectRequest(std::string host, int port) {
  // std::cout << "connect message: "<<host<<":" << port << std::endl;
  if (this->mUdpRemoteFunc)
    this->mUdpRemoteFunc(host,port);
}
