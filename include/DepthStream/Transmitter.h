//
//  This file is part of the ofxDepthStream [https://github.com/fusefactory/ofxDepthStream]
//  Copyright (C) 2018 Fuse srl
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

#pragma once

#include <stdlib.h>
#include <vector>
#include <sys/types.h>
#ifdef _WIN32
	#include <winsock.h>
#else
	#include <sys/socket.h>
	#include <netdb.h> //hostent
#endif

#include <thread>
#include "DepthStream/discover.h"

namespace depth {
  class BaseTransmitter;
  typedef std::shared_ptr<BaseTransmitter> BaseTransmitterRef;

  /**
   * \brief BaseTransmitter interface class
   */
  class BaseTransmitter {
    public:

      /// The constructor immediately starts the network server thread
      BaseTransmitter() { this->start(); }

      /// The destructor stops the network-server if it's still running
      ~BaseTransmitter() { this->stop(); }

      /// Transmits a the given frame-data if the network-server has a connected client
      virtual bool transmit(const void* data, size_t size) {return false;}

      virtual void start() {}

      /// Stops the network server
      virtual void stop(bool wait=true) {}
  };

  /**
   * \brief Network stream transmitter class
   *
   * The Transmitter class runs a network server on separate thread, which receives
   * incoming connection requests over TCP and starts sending a (compressed) stream
   * of Frames when a connection is established.
   */
  class UdpSocketTransmitter;
  typedef std::shared_ptr<UdpSocketTransmitter> UdpSocketTransmitterRef;
  
  class UdpSocketTransmitter : public BaseTransmitter {
    public:

      static UdpSocketTransmitterRef create(int port=4445) {
        return std::make_shared<UdpSocketTransmitter>(port);
      }

    public:

      /// The constructor immediately starts the network server thread
      UdpSocketTransmitter(int port=4445) : port(port) {}

      /// Transmits a the given frame-data if the network-server has a connected client
      bool transmit(const void* data, size_t size) override;

      void start() override;
      /// Stops the network server
      void stop(bool wait=true) override;

    protected:

      void error(const char *msg) {
          perror(msg);
      }

      void unbind();
      bool bind();
      void serverThread();

      bool transmitRaw(const void* data, size_t size);

    private:
      static const int MAXPACKETSIZE = 16;
      char packet[MAXPACKETSIZE];

      int port;
      std::thread *thread = NULL;

      bool bRunning=true;
      bool bBound=false;
      bool bConnected=false;

      int sockfd, clientsocket, portno;
      int cycleSleep=200;
  };

  // default transmitter class
  typedef UdpSocketTransmitter Transmitter;
  typedef std::shared_ptr<Transmitter> TransmitterRef;
}
