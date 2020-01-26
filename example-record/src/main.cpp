
#include <string>
#include <iostream>
#include "key_handler.h"
#include <DepthStream/DepthStream.h>

using namespace std;

const string HOST = "127.0.0.1";
const int PORT = 4445;

int main(int argc, char * argv[])
{
  if (argc < 2) {
    cout << "USAGE: DepthStreamRecord <file> [<host> [<port>]]" << std::endl;
    return 1;
  }

  string file = argv[1];
  string host = argc >= 3 ? argv[2] : HOST;
  int port = argc >= 4 ? stoi(argv[3]) : PORT;

  cout << "Starting listener on " << host << ":" << port << endl;
  auto receiverRef = depth::Receiver::createAndStart(host, port);
  // auto inflaterRef = std::make_shared<depth::Inflater>(1280 * 720 * 2 /* initial buffer size, optional */);
  depth::Recorder recorder;
  receiverRef->setOutputTo(&recorder);
  recorder.start(file);

  bool keepGoing = true;
  KeyHandler::set(&keepGoing);
 
  while(keepGoing) {
    // depth::emptyAndInflateBuffer(*receiverRef, [](const void* data, size_t size){
    //   //cout << "Received frame " << size << "-byte frame" << endl;
      
    // });
  }

  cout << "Shutting down." << endl;
  recorder.stop();
  return 0;
}