#include <cstdio>
#include <iostream>
#include <functional>
#include <lo/lo.h>
#include <lo/lo_cpp.h>

#include "discover/osc/PacketSender.h"

using namespace std;
using namespace std::placeholders;
using namespace discover;
using namespace discover::osc;
using namespace discover::osc::service;

void PacketSender::addUdpConsumer(const std::string& host, int port) {
  std::cout << "new consumer: "<<host<<":" << port << std::endl;
  ConsumerInfo i;
  i.host = host;
  i.port = to_string(port);
  this->consumers.push_back(i);
}

void PacketSender::submit(const void* data, size_t size) {
  for (auto& c : consumers) {
    // cout << "PacketSender::submit sending to "<<size<<" bytes to " << c.host << ":" << c.port << endl;
    lo::Address a(c.host, c.port);

    lo_blob blob = lo_blob_new(size, data);
    a.send(this->addr.c_str(), "b", blob);
  }
}
