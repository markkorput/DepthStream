#include <cstdio>
#include <iostream>
#include <functional>
#include <chrono>
#include <thread>

#include "discover/osc/PacketConsumer.h"

using namespace std;
using namespace discover;
using namespace discover::osc;

void PacketConsumer::start() {
  startBroadcastListeners();
}


void PacketConsumer::stop() {
  stopDataListener();
  stopBroadcastListeners();
}

void PacketConsumer::update() {
  processQueuedThreadOperations();
}

void PacketConsumer::onServiceFound(const std::string& host, int port) {
  if (dataServerHandle) return; // already listening for data

  stopBroadcastListeners();
  
  this->dataServerHandle = startDataListener();

  if (this->dataServerHandle) {
    this->sendConnectRequest(host, port);  
  } else {
    startBroadcastListeners();
  }
}

bool PacketConsumer::startBroadcastListeners() {
  for(auto port : broadcast::DEFAULT_PORTS) {
    auto handle = server::create(port);

    if (!handle) {
      cout << "Failed to create broadcast listener on port " << port << endl;
      continue;
    }

    broadcast::addServiceFoundCallback(handle, this->mServiceId, [this](std::string host, int port){
      this->onMainThread([this, host, port](){
        this->onServiceFound(host, port);
      });
    });

    server::start(handle);

    // cout << "Create broadcast listener: " << (long)handle << endl;;
    this->mBroadcastServerHandles.push_back(handle);
  }

  return mBroadcastServerHandles.size() > 0;
}

void PacketConsumer::stopBroadcastListeners() {
  for(auto handle : mBroadcastServerHandles) {
    server::destroy(handle);
  }

  mBroadcastServerHandles.clear();
}

server::InstanceHandle PacketConsumer::startDataListener() {
  // cout << "PacketConsumer::startDataListener" << endl;
  //
  // Create data receiver
  //
  auto server = server::create(mPort);

  if (!server) {
    cout << "Failed to create data listener, will not be able to receive data" << endl;
    return NULL;
  }

  packet::addCallback(server, [this](const void* data, size_t size){
    cout << "Received packet data" << endl;
    this->onData(data, size);
  });

  server::start(server);
  return server;
}

void PacketConsumer::stopDataListener() {
  if (this->dataServerHandle) {
    server::destroy(this->dataServerHandle);
    this->dataServerHandle = NULL;
  }
}

void PacketConsumer::sendConnectRequest(const std::string& serviceHost, int servicePort) {
  if (!dataServerHandle) return;
  auto url = server::get_url(dataServerHandle);
  connect::sendConsumerConnectRequest(mServiceId, serviceHost, servicePort, url);
}
