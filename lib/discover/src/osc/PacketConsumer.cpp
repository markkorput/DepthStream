#include <cstdio>
#include <iostream>
#include <functional>
#include <chrono>

#include "discover/osc/PacketConsumer.h"

using namespace std;
using namespace discover;
using namespace discover::osc;

void PacketConsumer::start() {
  this->dataServerHandle = server::create(mPort);

  if (!dataServerHandle) {
    cout << "Failed to create data listener, will not be able to receive data" << endl;
    return;
  }

  server::add_packet_callback(this->dataServerHandle, [this](const void* data, size_t size){
    this->onData(data, size);
  });

  
}

void PacketConsumer::stop() {
}
