#include <iostream>
#include <discover/osc/osc.h>
// #include <lo/lo.h>
#include <lo/lo_cpp.h>

using namespace std;

void discover::osc::broadcast_service(const string& serviceId, const vector<int>& ports, const string& url) {
  string addr="/discover/broadcast/"+serviceId;

  for (auto port : ports) {
    lo::Address a("255.255.255.255", port); 
    a.send(addr.c_str(), "s", url.c_str());
  }
}
