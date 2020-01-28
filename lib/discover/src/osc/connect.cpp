#include <iostream>
#include <discover/osc/osc.h>
#include <lo/lo_cpp.h>

using namespace std;
using namespace discover::osc;
using namespace discover::osc::server;

const string CONNECT_PREFIX = "/discover/connect/";

void discover::osc::connect::sendConsumerConnectRequest(const std::string& serviceId, const std::string& serviceHost, int servicePort, const std::string& consumerUrl) {
  string addr = CONNECT_PREFIX+serviceId;

  lo::Message m;
  m.add_string(consumerUrl);

  lo::Address(serviceHost, to_string(servicePort)).send(addr.c_str(), m);
}

void discover::osc::connect::addConnectRequestCallback(server::InstanceHandle server, const std::string& serviceId, ConsumerInfoCallback callback) {
  string addr = CONNECT_PREFIX+serviceId;
  
  auto st = (lo::ServerThread*)server;

  cout << "registering OSC handler for: " << addr << endl;


  st->add_method(addr, "s", [callback](lo_arg **argv, int) {
    // cout << "Got incoming connect request" << endl;
    std::string url = &argv[0]->s; // ie. "osc.udp://192.168.1.5:4445/"
    urlinfo info(url);
    callback(info.host, stoi(info.port));
  });
}