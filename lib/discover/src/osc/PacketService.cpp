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
  this->serviceConnectionListener = ServiceConnectionListener::start("depthframes", mPort,
    // udp connection listener; add consumer to sender
    [this](const std::string& host, int port) {
      osc::add_consumer(mConsumers, host, port);
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
}

void PacketService::update(uint32_t dtMs) {
  mUpdateTime += dtMs;

  if (this->serviceConnectionListener && mUpdateTime >= mNextBroadcastTime) {
    broadcast_service(mServiceId, mConnectionListenerUrl);
    mNextBroadcastTime = getTime() + mBroadcastIntervalMs;
  }
}

void PacketService::submit(const void* data, size_t size) {

  middleware::Packet initialpacket { data, size };
  middleware::Packet* packet = &initialpacket;

  for(auto& middleware : this->mMiddlewares) {
    packet = middleware(*packet);
    if (!packet) return; // middleware aborted
  }

  cout << "sending packet" << endl;
  osc::sendPacket(mConsumers, packet->data, packet->size, this->messageAddr);
}


void PacketService::add_middleware(discover::middleware::PacketMiddlewareFunc func) {
  if (!func) {
    cout << "PacketService.add_middleware received nullptr func";
    return;
  }

  mMiddlewares.push_back(func);
}