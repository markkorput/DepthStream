#include <iostream>
#include <discover/osc/osc.h>
// #include <lo/lo.h>
#include <lo/lo_cpp.h>

using namespace std;
using namespace discover::osc;

void discover::osc::sendPacket(const std::vector<ConsumerInfo>& consumers, const void* data, size_t size, const std::string& messageAddr) {
  for (auto& c : consumers) {
    // cout << "PacketSender::submit sending to "<<size<<" bytes to " << c.host << ":" << c.port << endl;
    lo::Address a(c.host, c.port);
    lo_blob blob = lo_blob_new(size, data);
    a.send(messageAddr.c_str(), "b", blob);
  }
}


using namespace discover::osc::ServiceConnectionListener;

Instance* discover::osc::ServiceConnectionListener::start(const std::string& serviceId, int port, ConsumerInfoCallback callback, int maxPortAttempts) {
  lo::ServerThread* st = (lo::ServerThread*)server::create(port, maxPortAttempts);

  string addr ="/discover/connect/"+serviceId;
  printf("registering OSC handler for: %s\n", addr.c_str());

  st->add_method(addr, "si", [callback](lo_arg **argv, int) {
    string host = &argv[0]->s;
    int port = argv[1]->i;
    callback(host, port);
  });

  st->start();

  return (Instance*) st;
}
