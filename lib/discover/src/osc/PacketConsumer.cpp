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

void PacketConsumer::onServiceFound(const std::string& host, int port) {
  if (dataServerHandle) return;

  stopBroadcastListeners();
  
  if (startDataListener()) {
      sendConnectRequest(host, port);
  } else {
    startBroadcastListeners();
  }
}

void PacketConsumer::stop() {
  stopDataListener();
  stopBroadcastListeners();
}

void PacketConsumer::update() {
  if (this->mutex.try_lock()) {
    std::vector<std::function<void()>> copy = mUpdateFuncs;
    mUpdateFuncs.clear();
    this->mutex.unlock();

    for (auto func : copy)
      func();
   
  }
}

bool PacketConsumer::startBroadcastListeners() {
  for(auto port : broadcast::DEFAULT_PORTS) {
    auto handle = server::create(port);

    if (!handle) {
      cout << "Failed to create broadcast listener on port " << port << endl;
      continue;
    }

    broadcast::add_service_found_callback(handle, this->mServiceId, [this](std::string host, int port){
      this->mutex.lock();
      { 
        this->onMainThread([this, host, port](){
          this->onServiceFound(host, port);
        });
      }
      this->mutex.unlock();
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

bool PacketConsumer::startDataListener() {
  // cout << "PacketConsumer::startDataListener" << endl;
  //
  // Create data receiver
  //
  this->dataServerHandle = server::create(mPort);

  if (!dataServerHandle) {
    cout << "Failed to create data listener, will not be able to receive data" << endl;
    return false;
  }

  server::add_packet_callback(this->dataServerHandle, [this](const void* data, size_t size){
    this->onData(data, size);
  });

  server::start(this->dataServerHandle);
  return true;
}

void PacketConsumer::stopDataListener() {
  if (this->dataServerHandle) {
    server::destroy(this->dataServerHandle);
    this->dataServerHandle = NULL;
  }
}

void PacketConsumer::sendConnectRequest(const std::string& host, int port) {

}