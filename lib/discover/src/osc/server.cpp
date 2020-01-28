#include <iostream>
#include <discover/osc/osc.h>
// #include <lo/lo.h>
#include <lo/lo_cpp.h>

using namespace std;
using namespace discover::osc;
using namespace discover::osc::server;

InstanceHandle discover::osc::server::create(int port, int maxPortAttempts, bool start) {
  lo::ServerThread* st;

  // starting server
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

  if (start) 
    st->start();

  return st;
}

void discover::osc::server::start(InstanceHandle instance) {
  ((lo::ServerThread*)instance)->start();
}

bool discover::osc::server::destroy(InstanceHandle instance) {
  if (instance == NULL) return false;
  lo_server_thread_free((lo::ServerThread*)instance);
  return true;
}

const std::string discover::osc::server::get_url(InstanceHandle instance) {
  return ((lo::ServerThread*)instance)->url();
}


void discover::osc::server::add_packet_callback(server::InstanceHandle serverHandle, DataFunc callback) {
  string addr ="/data";

  auto st = (lo::ServerThread*)serverHandle;

  printf("registering OSC handler for: %s\n", addr.c_str());
  st->add_method(addr, "b", [callback](lo_arg **argv, int) {
    void* data = &argv[0]->blob.data;
    size_t size = argv[0]->blob.size;
    callback(data, size);
  });
}