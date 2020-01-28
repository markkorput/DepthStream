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
  auto server = server::create(mPort);

  if (!server) {
    return;
  }

  connect::addConnectRequestCallback(server, "depthframes",
    // udp connection listener; add consumer to sender
    [this](const std::string& host, int port) {
      // cout << "adding consumer: " << port <<endl;
      osc::connect::addConsumer(mConsumers, host, port);
    });

  mConnectionListenerUrl = server::get_url(server);

  this->connectionListener = server;
  server::start(server);
  
  // start broadcast interval
  mUpdateTime = mNextBroadcastTime = getTime();
}

void PacketService::stop() {
  if (connectionListener) {
    server::destroy(connectionListener);
    connectionListener = NULL;
  }
}

void PacketService::update(uint32_t dtMs) {
  mUpdateTime += dtMs;

  if (this->connectionListener && mUpdateTime >= mNextBroadcastTime) {
    broadcast::announce(mServiceId, mConnectionListenerUrl);
    mNextBroadcastTime = getTime() + mBroadcastIntervalMs;
  }
}

void PacketService::submit(const void* data, size_t size) {
  // cout << "PacketService::submit size=" << size << endl;
  middleware::Packet initialpacket { data, size };
  middleware::Packet* packet = &initialpacket;

  for(auto& middleware : this->mMiddlewares) {
    packet = middleware(*packet);
    if (!packet) return; // middleware aborted
  }

  // cout << "sending packet (size=" << packet->size << ")" << endl;
  osc::packet::send(mConsumers, packet->data, packet->size);
}


void PacketService::add_middleware(discover::middleware::PacketMiddlewareFunc func) {
  if (!func) {
    cout << "PacketService.add_middleware received nullptr func";
    return;
  }

  mMiddlewares.push_back(func);
}