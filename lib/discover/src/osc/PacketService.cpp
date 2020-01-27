#include <cstdio>
#include <iostream>
#include <functional>

#include "discover/osc/PacketService.h"

using namespace std;
using namespace std::placeholders;
using namespace discover;
using namespace discover::osc;
using namespace discover::osc::service;

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
