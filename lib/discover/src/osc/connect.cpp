#include <iostream>
#include <discover/osc/osc.h>
// #include <lo/lo.h>
#include <lo/lo_cpp.h>

using namespace std;
using namespace discover::osc;
using namespace discover::osc::server;

const string CONNECT_PREFIX = "/discover/connect/";

void discover::osc::connect::sendConsumerConnectRequest(const std::string& serviceId, const std::string& serviceHost, int servicePort, const std::string& consumerUrl) {
  string addr = CONNECT_PREFIX+serviceId;
  
  lo::Message m;
  m.add_string(consumerUrl);

  lo::Address a(serviceHost, to_string(servicePort));
  a.send(addr.c_str(), m);
}