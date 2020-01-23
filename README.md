# DepthStream

## Usage

_See also the example applications in this repository_

### Transmit frames of data using TransmitterAgent
_TransmitterAgent is a helper class that takes care of compression, throttling and potential conversions_

```c++

  size_t readFrameData(char* buffer) {
    // CUSTOM DATA FETCHING CODE HERE
  }

  char buffer[1024*1024*512];
  int port = 4445; // default
  depth::TransmitterAgent transmitter(port);

  while(true) {
    auto frame_size = readFrameData((void**)buffer);
    
    if (frame_size > 0) {
      transmitter.submit(buffer, frame_size);
    }
  }
```

### Receive frames of data

```c++
  auto receiverRef = depth::Receiver::createAndStart("127.0.0.1"); // uses default port 4445

  while(true) {
    depth::emptyAndInflateBuffer(*receiverRef, [](const void* data, size_t size){
      std::cout << "Received frame " << size << "-byte frame" << std::endl;
    });
  }
```
