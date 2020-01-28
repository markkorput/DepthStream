#pragma once

#include <stdlib.h>
#include <memory>
#include <string>
#include <chrono>
#include <math.h>
#include <mutex>

#include "osc.h"
#include <discover/middleware.h>

namespace discover { namespace osc {

  class PacketConsumer;
  typedef std::shared_ptr<PacketConsumer> PacketConsumerRef;

  class PacketConsumer {

    public: // types

      typedef std::function<void(const void*, size_t)> DataHandler;

    public: // static factory methods

      static PacketConsumerRef create(const std::string& serviceId, DataHandler func, bool start=true) {
        auto ref = std::make_shared<PacketConsumer>(serviceId, false);
        
        ref->dataCallback = func;

        if (start)
          ref->start();

        return ref;
      }

    public: // methods

      PacketConsumer(const std::string& serviceId, bool start=true) : mServiceId(serviceId) {
        if (start) {
          this->start();
        }
      }

      ~PacketConsumer() { stop(); }

      void start();
      void stop();
      void update();

    protected:

      inline void onData(const void* data, size_t size) {
        if (this->dataCallback) this->dataCallback(data, size);
      }

      void onServiceFound(const std::string& host, int port);

      bool startBroadcastListeners();
      void stopBroadcastListeners();

      server::InstanceHandle startDataListener();
      void stopDataListener();

      void sendConnectRequest(const std::string& serviceHost, int servicePort);

      inline void onMainThread(std::function<void()> func) {
        this->mutex.lock();
        mUpdateFuncs.push_back(func);
        this->mutex.unlock();
      }

      inline void processQueuedThreadOperations() { 
        // anything to do?
        if (mUpdateFuncs.size() < 1) return;

        // can we get a mutex lock?
        if (!this->mutex.try_lock()) return;

        // copy funcs vector
        std::vector<std::function<void()>> funcs = mUpdateFuncs;

        // clear main list
        mUpdateFuncs.clear();

        // unlock mutex
        this->mutex.unlock();

        // execute operations
        for (auto func : funcs)
          func();
      }

    private:

      std::string mServiceId;
      std::string messageAddr = "/frame";
      int mPort=4445; // default UDP port on which to receive data

      packet::Buffer buffer;
      DataHandler dataCallback = nullptr;
      server::InstanceHandle dataServerHandle = NULL;
      std::vector<server::InstanceHandle> mBroadcastServerHandles;

      std::mutex mutex;
      std::vector<std::function<void()>> mUpdateFuncs;
  };
}}
