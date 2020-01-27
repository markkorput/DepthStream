#include <iostream>
#include <discover/osc/osc.h>
// #include <lo/lo.h>
#include <lo/lo_cpp.h>


using namespace std;
void discover::osc::broadcast_service(const string& serviceId, int port, const std::string& url) {
  lo::Address a("255.255.255.255", port);
  std::string addr="/discover/broadcast/"+serviceId;
  a.send(addr.c_str(), "s", url.c_str());
}