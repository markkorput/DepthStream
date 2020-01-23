#include <cstdio>
#include <chrono>
#include <iostream>
#include <iomanip>

#include "Compressor.h"
#include "Transmitter.h"

namespace depth {

  class Converter16to32bit {
    public:
      bool convert(const void* data, size_t size);
      const void* getData(){ return (void*)buffer; }
      size_t getSize(){ return lastSize; }

    private:
      static const size_t BUF_SIZE = (1280*720*4);
      unsigned char buffer[BUF_SIZE];
      size_t lastSize=0;
  };

  class TransmitterAgent {

    public:
      TransmitterAgent(unsigned int port=4445, float fps=60.0f, std::shared_ptr<Converter16to32bit> converter=nullptr, bool verbose=false);
      TransmitterAgent(int argc, char** argv);
      bool submit(const void* data, size_t size);
      inline bool getVerbose() const { return bVerbose; }
      inline void setDoCompress(bool v) { bCompress = v; }

    protected:

      void configure(int argc, char** argv);
      bool transmitFrame(const void* data, size_t size);

    private:

      int depthPort = 4445;
      bool bVerbose=false;
      std::shared_ptr<Converter16to32bit> converterRef = nullptr;

      bool bCompress = true;
      depth::CompressorRef compressor=nullptr;
      depth::TransmitterRef transmitter=nullptr;

      using clock_type = std::chrono::system_clock;
      unsigned int frameMs = (int)(1.0f/(float)60.0 * 1000.0f); // milliseconds
      std::chrono::time_point<clock_type> nextFrameTime;
  };
}