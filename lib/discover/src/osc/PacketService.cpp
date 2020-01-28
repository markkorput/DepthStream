#include <cstdio>
#include <iostream>
#include <functional>
#include <chrono>

#include "discover/osc/PacketService.h"
#include "discover/osc/osc.h"

using namespace std;
using namespace std::placeholders;
using namespace discover;
using namespace discover::osc;
using namespace discover::osc::service;

void PacketService::start() {
  this->packetSenderRef = PacketSender::create();

  this->serviceConnectionListener = ServiceConnectionListener::start("depthframes", mPort,
    // udp connection listener; add consumer to sender
    [this](const std::string& host, int port) {
      this->packetSenderRef->addUdpConsumer(host,port);
    });
  mConnectionListenerUrl = ServiceConnectionListener::get_url(this->serviceConnectionListener);
  
  // start broadcast interval
  mUpdateTime = mNextBroadcastTime = getTime();
}

void PacketService::stop() {
  if (serviceConnectionListener) {
    ServiceConnectionListener::stop(serviceConnectionListener);
    serviceConnectionListener = NULL;
  }

  if (this->packetSenderRef) {
    this->packetSenderRef = nullptr;
  }
}

void PacketService::update(uint32_t dtMs) {
  mUpdateTime += dtMs;

  if (this->serviceConnectionListener && mUpdateTime >= mNextBroadcastTime) {
    broadcast_service(mServiceId, mConnectionListenerUrl);
    mNextBroadcastTime = getTime() + mBroadcastIntervalMs;
  }
}

void PacketService::submit(const void* data, size_t size) {
  if (this->packetSenderRef)
    this->packetSenderRef->submit(data,size);
}
