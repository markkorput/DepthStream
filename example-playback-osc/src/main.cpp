
#include <string>
#include <iostream>
#include "key_handler.h"
#include <DepthStream/DepthStream.h>

using namespace std;

int main(int argc, char * argv[])
{
  if (argc < 2) {
    cout << "USAGE: DepthStreamPlayback <file>" << std::endl;
    return 1;
  }

  string file = argv[1];
  int port = 4445; // port on which to receive new connections

  discover::osc::service::PacketService service("depthframes", port);

  cout << "Starting player with file " << file << endl;
  depth::Playback playback;

  playback.start(file);

  bool keepGoing = true;
  KeyHandler::set(&keepGoing);
 
  while(keepGoing) {
    playback.update([&service](void* data, size_t size){
      service.submit(data, size);
    });
  }

  cout << "Shutting down." << endl;
  playback.stop();
  service.stop();
  return 0;
}