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
        if (this->dataCallback)
          this->dataCallback(data, size);
      }

      void onServiceFound(const std::string& host, int port);

      bool startBroadcastListeners();
      void stopBroadcastListeners();

      bool startDataListener();
      void stopDataListener();

      void sendConnectRequest(const std::string& host, int port);

      inline void onMainThread(std::function<void()> func) {
        mUpdateFuncs.push_back(func);
      }

    private:

      std::string mServiceId;
      std::string messageAddr = "/frame";
      int mPort=4445; // default UDP port on which to receive data

      DataHandler dataCallback = nullptr;
      server::InstanceHandle dataServerHandle = NULL;
      std::vector<server::InstanceHandle> mBroadcastServerHandles;

      struct {
        std::string host = "";
        int port = 0;
      } mServiceInfo;

      std::mutex mutex;

      std::vector<std::function<void()>> mUpdateFuncs;
  };
}}
