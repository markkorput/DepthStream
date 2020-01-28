
#include <string>
#include "key_handler.h"
#include <DepthStream/DepthStream.h>

using namespace std;

const string HOST = "127.0.0.1";
const int PORT = 4445;

int main(int argc, char * argv[])
{
  string host = HOST;
  int port = PORT;
  bool useConsumer = true;

  if (argc == 3) {
    host = argv[1];
    port = stoi(argv[2]);
    useConsumer = false;
  } else if (argc == 2) {
    port = stoi(argv[1]);
  } else if (argc == 1) {
    useConsumer = false;
  } else {
    cout << "USAGE:"<<endl<<"DepthStreamDump <host> <port>"<<endl<<"DepthStreamDump <port>" <<endl;
    return 0;
  }


  depth::ReceiverRef receiverRef = nullptr;
  
  depth::Buffer buffer;

  if (useConsumer) {
  } else {
    cout << "Creating receiver on: " << host << ":" << port << endl;
    receiverRef = depth::Receiver::createAndStart(host, port);
    auto inflaterRef = std::make_shared<depth::Inflater>(1280 * 720 * 2 /* initial buffer size, optional */);
    // receiverRef->setOutputTo(buffer);
    receiverRef->setOutput([inflaterRef, &buffer](const void* data, size_t size){
      if (inflaterRef->inflate(data, size)) {
        buffer.write(inflaterRef->getData(), inflaterRef->getSize());
      }
    });
  }

  bool keepGoing = true;
  KeyHandler::set(&keepGoing);
 

  while(keepGoing) {
    depth::emptyBuffer(buffer, [](const void* data, size_t size){
      cout << "Received frame: " << size << " bytes" << endl;
    });
  }

  cout << "Shutting down." << endl;
  return 0;
}