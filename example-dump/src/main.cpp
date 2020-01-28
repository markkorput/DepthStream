
#include <string>
#include "key_handler.h"
#include <DepthStream/DepthStream.h>
#include <discover/osc/PacketConsumer.h>

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
  } else if (argc < 2) {
    // useConsumer = true;
  } else {
    cout << "USAGE:"<<endl<<"DepthStreamDump <host> <port>"<<endl<<"DepthStreamDump <port>" <<endl;
    return 0;
  }


  depth::ReceiverRef receiverRef = nullptr;
  depth::Buffer buffer;
  discover::osc::PacketConsumerRef packetConsumerRef = nullptr;
  auto inflaterRef = std::make_shared<depth::Inflater>(1280 * 720 * 2 /* initial buffer size, optional */);
  
  auto datahandler = [inflaterRef, &buffer](const void* data, size_t size){
    cout << "inflating " << size << " bytes" << endl;
    if (inflaterRef->inflate(data, size)) {
      cout << "write to buf" << endl;
      buffer.write(inflaterRef->getData(), inflaterRef->getSize());
    }
  };


  if (useConsumer) {
    packetConsumerRef = discover::osc::PacketConsumer::create("depthframes", datahandler);
  } else {
    cout << "Creating receiver on: " << host << ":" << port << endl;
    receiverRef = depth::Receiver::createAndStart(host, port);
    receiverRef->setOutput(datahandler);
  }

  bool keepGoing = true;
  KeyHandler::set(&keepGoing);
 

  while(keepGoing) {
    if (packetConsumerRef) packetConsumerRef->update();
    depth::emptyBuffer(buffer, [](const void* data, size_t size){
      cout << "Received frame: " << size << " bytes" << endl;
    });
  }

  cout << "Shutting down." << endl;
  return 0;
}