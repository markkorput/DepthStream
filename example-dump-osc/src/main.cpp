
#include <string>
#include <functional>
#include "key_handler.h"
#include <DepthStream/DepthStream.h>
#include "DepthStream/discover.h"

using namespace std;

const string HOST = "127.0.0.1";
const int PORT = 4445;


namespace discover {

  class FrameConsumer;
  typedef std::shared_ptr<FrameConsumer> FrameConsumerRef;

  class FrameConsumer {
    public:
    typedef std::function<void(const void*, size_t)> DataFunc;
    
    public:
      void onData(DataFunc func) { mDataFunc = func; }
    
    private :
      DataFunc mDataFunc;
  };

  class ServiceFinder;
  typedef std::shared_ptr<ServiceFinder> ServiceFinderRef;

  class ServiceFinder {
    public:
      static ServiceFinderRef create(std::string serviceId, FrameConsumerRef consumer=nullptr) {
        return std::make_shared<ServiceFinder>(serviceId, consumer);
      }

      static ServiceFinderRef create(std::string serviceId, FrameConsumer::DataFunc consumerFunc) {
        auto consumer = std::make_shared<FrameConsumer>();
        consumer->onData(consumerFunc);
        return ServiceFinder::create(serviceId, consumer);
      }

    public:
      ServiceFinder(std::string serviceId, FrameConsumerRef consumer=nullptr) : mServiceId(serviceId), consumerRef(consumer){
        this->start();
      }

      void start() {
        //TODO: start listening for service broadcasts

        // connect to default addr
        // start listener for incoming data
      }

      void stop() {

      }

    private:
      std::string mServiceId;
      FrameConsumerRef consumerRef;
  };
}


int main(int argc, char * argv[])
{
  depth::Buffer buffer;
  
  
  // auto receiverRef = depth::OscReceiver::createAndStart();
  auto inflaterRef = std::make_shared<depth::Inflater>(1280 * 720 * 2 /* initial buffer size, optional */);


  bool keepGoing = true;
  KeyHandler::set(&keepGoing);

  auto finder = discover::ServiceFinder::create("depthframe", 
    // service consumer logic
    [&buffer](const void* data, size_t size) {
      buffer.write(data,size);
    });

  while(keepGoing) {
    depth::emptyAndInflateBuffer(buffer, [](const void* data, size_t size){
      cout << "Received frame " << size << "-byte frame" << endl;
    }, depth::Opts().useInflater(inflaterRef));
  }

  cout << "Shutting down." << endl;
  return 0;
}