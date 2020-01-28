#include <iostream>
#include <discover/osc/osc.h>
// #include <lo/lo.h>
#include <lo/lo_cpp.h>

using namespace std;

void discover::osc::broadcast::announce(const string& serviceId, const vector<int>& ports, const string& url) {
  string addr="/discover/broadcast/"+serviceId;

  for (auto port : ports) {
    lo::Address a("255.255.255.255", port); 
    a.send(addr.c_str(), "s", url.c_str());
  }
}

void discover::osc::broadcast::addServiceFoundCallback(server::InstanceHandle server, const std::string& serviceId, ServiceInfoFunc callback) {
  string addr="/discover/broadcast/"+serviceId;

  auto st = (lo::ServerThread*)server;

  printf("registering OSC handler for: %s\n", addr.c_str());
  st->add_method(addr, "s", [callback](lo_arg **argv, int) {
    std::string url = &argv[0]->s; // ie. "osc.udp://markbookproretina.local:4445/"
    // cout << "Received service broadcast: " << url << endl;
    urlinfo info(url);
    // cout << "protocol=" <<info.protocol<<" host="<<info.host<<" port="<<info.port<<endl;
    callback(info.host, stoi(info.port));
  });
}