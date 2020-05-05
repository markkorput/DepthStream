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

  depth::TransmitterAgent agent(argc, argv);
  // recorded data is already compressed
  agent.setDoCompress(false);

  depth::InflaterRef inflaterRef=nullptr;

  for (int i=0; i<argc; i++) {
    if (strcmp(argv[i], "--inflated") == 0
    || strcmp(argv[i], "--inflate") == 0
    || strcmp(argv[i], "--decompress") == 0) {
      cout << "enabling inflater..." << endl;
      inflaterRef = std::make_shared<depth::Inflater>();
      break;
    }
  }


  string file = argv[1];
  cout << "Starting player with file " << file << endl;
  depth::Playback playback;

  playback.start(file);



  bool keepGoing = true;
  KeyHandler::set(&keepGoing);
 
  while(keepGoing) {
    playback.update([&agent, inflaterRef](const void* data, size_t size){

      // inflate first? (to transmit uncompressed data)
      if (inflaterRef) {
        if (!inflaterRef->inflate(data, size)) {
          cerr << "failed to inflate frame" << endl;
          return;
        }

        data = inflaterRef->getData();
        size = inflaterRef->getSize();
      }

      if (agent.getVerbose()) cout << size << "-byte frame" << endl;
      agent.submit(data,size);
    });
  }

  cout << "Shutting down." << endl;
  playback.stop();
  return 0;
}