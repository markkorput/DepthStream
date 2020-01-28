
#include <string>
#include <iostream>
#include "key_handler.h"
#include <DepthStream/DepthStream.h>
#include <discover/all.h>

using namespace std;

int main(int argc, char * argv[])
{
  if (argc < 2) {
    cout << "USAGE: DepthStreamPlayback <file>" << std::endl;
    return 1;
  }

  string file = argv[1];
  int port = argc >= 3 ? stoi(argv[2]) : 4445;

  // Start packet-sending service, identifier "depthframes", accepting
  // new connections on port-number: <port>
  discover::osc::service::PacketService service("depthframes", port);

  service.add_middleware(discover::middleware::throttle_max_fps(1)); // max 30 fps
  // service.add_middleware(discover::middleware::convert16to32bit()); // perform some conversion?
  service.add_middleware(discover::middleware::compress()); // deflate data

  cout << "Starting player with file " << file << endl;
  depth::Playback playback;
  playback.start(file);

  bool keepGoing = true;
  KeyHandler::set(&keepGoing);
 
  while(keepGoing) {
    service.update();
    playback.update([&service](void* data, size_t size){
      service.submit(data, size);
    });
  }

  cout << "Shutting down." << endl;
  playback.stop();
  service.stop();
  return 0;
}