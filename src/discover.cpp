#include <cstdio>
#include <iostream>
#include <functional>
#include <lo/lo.h>
#include <lo/lo_cpp.h>

#include "DepthStream/discover.h"

using namespace std;
using namespace std::placeholders;
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



void PacketSender::addUdpConsumer(const std::string& host, int port) {
  std::cout << "new consumer: "<<host<<":" << port << std::endl;
  ConsumerInfo i;
  i.host = host;
  i.port = to_string(port);
  this->consumers.push_back(i);
}

void PacketSender::submit(const void* data, size_t size) {
  for (auto& c : consumers) {
    // cout << "PacketSender::submit sending to "<<size<<" bytes to " << c.host << ":" << c.port << endl;
    lo::Address a(c.host, c.port);

    lo_blob blob = lo_blob_new(size, data);
    a.send(this->addr.c_str(), "b", blob);
  }
}

void PacketService::start() {
  this->packetSenderRef = PacketSender::create();

  this->serviceConnectionListenerRef = ServiceConnectionListener::create("depthframes", mPort,
    // udp connection listener; add consumer to sender
    [this](const std::string& host, int port) {
      this->packetSenderRef->addUdpConsumer(host,port);
    });
}

void PacketService::stop() {
  if (this->serviceConnectionListenerRef) {
    this->serviceConnectionListenerRef->stop();
    this->serviceConnectionListenerRef = nullptr;
  }

  if (this->packetSenderRef) {
    this->packetSenderRef = nullptr;
  }
}

void PacketService::submit(const void* data, size_t size) {
  if (this->packetSenderRef)
    this->packetSenderRef->submit(data,size);
}
