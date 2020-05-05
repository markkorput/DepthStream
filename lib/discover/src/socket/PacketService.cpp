#include <cstdio>
#include <iostream>
#include <functional>
#include <chrono>

#include "discover/socket/PacketService.h"
#include "discover/socket.h"

using namespace std;
using namespace discover;
using namespace discover::socket;

void PacketService::start() {
  auto server = server::create(mPort, [this](int new_consumer_socket) {
    // cout << "adding consumer: " << port <<endl;
    connect::add_consumer(mConsumers, new_consumer_socket);
  });

  if (!server) {
    return;
  }

  // mConnectionListenerUrl = server::get_url(server);

  // this->connectionListener = server;
  // server::start(server);
  
  // // start broadcast interval
  // mUpdateTime = mNextBroadcastTime = getTime();
}

void PacketService::stop() {
  // if (connectionListener) {
  //   server::destroy(connectionListener);
  //   connectionListener = NULL;
  // }
}

void PacketService::update(uint32_t dtMs) {
  mUpdateTime += dtMs;

  // if (this->connectionListener && mUpdateTime >= mNextBroadcastTime) {
    // broadcast::announce(mServiceId, mConnectionListenerUrl);
    mNextBroadcastTime = getTime() + mBroadcastIntervalMs;
  // }
}

// void PacketService::submit(const void* data, size_t size) {
//   // cout << "sending packet (size=" << packet->size << ")" << endl;
//   osc::packet::send(mConsumers, packet->data, packet->size);
// }
