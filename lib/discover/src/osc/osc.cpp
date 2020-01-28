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
  lo::ServerThread* st;

  { // starting server
    int maxport = port + maxPortAttempts;

    do {
      printf("Attempting to start OSC server on port: %i\n", port);
      st = new lo::ServerThread(port);
      if (st->is_valid()) break;
      port++;
    } while(port < maxport);

    if (!st->is_valid()) {
      std::cout << "Failed to create valid lo::ServerThread. (ports already in use by other services)" << std::endl;
      return NULL;
    }
  }

  // st->set_callbacks([&st](){
  //     printf("Thread init: %p.\n",&st);},
  //     [](){printf("Thread cleanup.\n");});

  // std::cout << "URL: " << st->url() << std::endl;
  // this->mUrl = st->url();

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

bool discover::osc::ServiceConnectionListener::stop(Instance* instance) {
  if (instance == NULL) return false;
  lo_server_thread_free((lo::ServerThread*)instance);
  return true;
}

const std::string discover::osc::ServiceConnectionListener::get_url(Instance* instance) {
  return ((lo::ServerThread*)instance)->url();
}