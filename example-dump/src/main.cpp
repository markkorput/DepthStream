
#include <string>
#include "key_handler.h"
#include <DepthStream/DepthStream.h>

using namespace std;

const string HOST = "127.0.0.1";
const int PORT = 4445;

int main(int argc, char * argv[])
{
  auto receiverRef = depth::Receiver::createAndStart(HOST, PORT);
  auto inflaterRef = std::make_shared<depth::Inflater>(1280 * 720 * 2 /* initial buffer size, optional */);


  bool keepGoing = true;
  KeyHandler::set(&keepGoing);
 
  while(keepGoing) {
    depth::emptyAndInflateBuffer(*receiverRef, [](const void* data, size_t size){
      cout << "Received frame " << size << "-byte frame" << endl;
    });
  }

  cout << "Shutting down." << endl;
  return 0;
}