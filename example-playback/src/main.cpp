
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

  depth::TransmitterAgent agent(argc, argv);
  agent.setDoCompress(false);


  cout << "Starting player with file " << file << endl;
  depth::Playback playback;

  playback.start(file);

  bool keepGoing = true;
  KeyHandler::set(&keepGoing);
 
  while(keepGoing) {
    playback.update([&agent](void* data, size_t size){
      if (agent.getVerbose()) cout << size << "-byte frame" << endl;
      agent.submit(data,size);
    });
  }

  cout << "Shutting down." << endl;
  playback.stop();
  return 0;
}