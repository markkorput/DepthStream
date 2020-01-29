
#include <string>
#include <iostream>
#include "key_handler.h"
#include <DepthStream/DepthStream.h>
#include <discover/all.h>
#include <DepthStream/compression.h>

using namespace std;


int main(int argc, char * argv[])
{
  if (argc < 2) {
    cout << "USAGE: DepthStreamPlayback <file> [<port>]" << std::endl;
    return 1;
  }

  string file = argv[1];
  int port = argc >= 3 ? stoi(argv[2]) : 4445;


  // Start packet-sending service, identifier "depthframes", accepting
  // new connections on port-number: <port>
  discover::osc::PacketService service("depthframes", port);

  // create throttle middleware step; only allows 1 packet per second
  auto throttle = discover::middleware::packet::throttle_max_fps(1);

  // compression step; compresses data
  depth::compression::CompressBuffer buf;
  auto compress = depth::compression::middleware::compress(buf);

  // send step; submits data to the service
  auto send = discover::middleware::packet::to_step([&service](const void* data, size_t size) -> bool {
    service.submit(data, size);
    return true;
  });

  auto log = [](const char* prefix) {
    return discover::middleware::packet::to_step([prefix](const void* data, size_t size) -> bool {
      cout << prefix << " " << size << " bytes" << endl;
      return true;
    });
  };

  cout << "Starting player with file " << file << endl;
  depth::Playback playback;
  playback.start(file);

  bool keepGoing = true;
  KeyHandler::set(&keepGoing);
 
  while(keepGoing) {
    service.update();

    if (playback.update()) {
      discover::middleware::start(playback.getRef()->data(), playback.getRef()->size())
        | throttle
        | compress
        | send
        | log("submitted")
        ;
    }
  }

  cout << "Shutting down." << endl;
  playback.stop();
  service.stop();
  return 0;
}