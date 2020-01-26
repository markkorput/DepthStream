#include <cstdio>
#include <iostream>
#include <functional>
#include <lo/lo.h>
#include <lo/lo_cpp.h>

#include "DepthStream/discover.h"

using namespace std;
using namespace std::placeholders;
using namespace discover;

ServiceProviderRef ServiceProvider::create(const string& serviceTypeId, ServiceRef service) {
  return make_shared<ServiceProvider>(serviceTypeId, service);
}

void ServiceProvider::start() {
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

void ServiceProvider::stop() {
  if (serverThread) {
    lo_server_thread_free((lo::ServerThread*)serverThread);
    serverThread = NULL;
  }
}

void ServiceProvider::onConnectRequest(std::string host, int port) {
  // std::cout << "connect message: "<<host<<":" << port << std::endl;
  
  if (this->serviceRef) {
    this->serviceRef->addUdpConsumer(host, port);
  }
}

void OscFrameService::addUdpConsumer(const std::string& host, int port) {
  std::cout << "new consumer: "<<host<<":" << port << std::endl;
  ConsumerInfo i;
  i.host = host;
  i.port = to_string(port);
  this->consumers.push_back(i);
}

void OscFrameService::submit(const void* data, size_t size) {
  for (auto& c : consumers) {
    // cout << "OscFrameService::submit sending to "<<size<<" bytes to " << c.host << ":" << c.port << endl;
    lo::Address a(c.host, c.port);

    lo_blob blob = lo_blob_new(size, data);
    a.send(this->addr.c_str(), "b", blob);
  }
}