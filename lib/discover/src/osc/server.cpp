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
    cout << "Attempting to start OSC server on port: "<< port << endl;
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
  cout << "Destroying server: "<< ((lo::ServerThread*)instance)->url() << endl;
  if (instance == NULL) return false;
  auto st = (lo::ServerThread*)instance;
  delete st;
  return true;
}

const std::string discover::osc::server::get_url(InstanceHandle instance) {
  return ((lo::ServerThread*)instance)->url();
}
