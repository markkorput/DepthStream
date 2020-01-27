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
  printf("starting OSC server on port: %i\n", this->mPort);
  auto st = new lo::ServerThread(mPort);

  if (!st->is_valid()) {
      std::cout << "Faield to create valid lo::ServerThread." << std::endl;
      return;
  }

  // st->set_callbacks([&st](){
  //     printf("Thread init: %p.\n",&st);},
  //     [](){printf("Thread cleanup.\n");});

  // std::cout << "URL: " << st->url() << std::endl;
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
