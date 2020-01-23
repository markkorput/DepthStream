#include <cstdio>
#include <chrono>
#include <iostream>
#include <iomanip>

#include "DepthStream/TransmitterAgent.h"

using namespace depth;

bool Converter16to32bit::convert(const void* data, size_t size) {
  if((size * 2) > BUF_SIZE){
    std::cerr << "Convert: " << size << " bytes is too big for our buffer" << std::endl;
    return false;
  }

  memset(buffer, 0, BUF_SIZE);

  size_t index=0;
  size_t ttl = size/2;
  while(index < ttl) {
    uint16_t val = ((uint16_t*)data)[index];

    // std::cout << "convert iteration: " << index << "," << ttl << "siz2: " << sizeof(uint32_t) << std::endl;
    ((unsigned char*)buffer)[index * 4 + 0] = 0;
    ((unsigned char*)buffer)[index * 4 + 1] = 0;
    ((unsigned char*)buffer)[index * 4 + 2] = (unsigned char)(val >> 8 & 0xFF);
    ((unsigned char*)buffer)[index * 4 + 3] = (unsigned char)(val & 0xFF);

    index += 1;

    // ((uint32_t*)this->buffer)[index*2] = ((uint16_t*)data)[index];
    // ((uint16_t*)this->buffer)[index*2+1] = 0;
    // index += 1;
  }

  lastSize = size * 2; //destCursor;
  // std::cout << "Converting done, original size: " << size << ", ttl: " << ttl << ", last size: " << lastSize << std::endl;
  return true;
}

TransmitterAgent::TransmitterAgent(unsigned int port, float fps, std::shared_ptr<Converter16to32bit> converter, bool verbose)
  : depthPort(port), bVerbose(verbose), converterRef(converter) {
    frameMs = 1.0f / fps * 1000.0f;
    nextFrameTime = std::chrono::system_clock::now();
    this->compressor = std::make_shared<depth::Compressor>();
    this->transmitter = std::make_shared<depth::Transmitter>(this->depthPort);
}

TransmitterAgent::TransmitterAgent(int argc, char** argv) {
  frameMs = 1.0f / 60.0f * 1000.0f;
  this->configure(argc, argv);
  nextFrameTime = std::chrono::system_clock::now();
  this->compressor = std::make_shared<depth::Compressor>();
  this->transmitter = std::make_shared<depth::Transmitter>(this->depthPort);
}

bool TransmitterAgent::submit(const void* data, size_t size) {
  auto t = std::chrono::system_clock::now();

  // not yet time for next frame?
  if (t < nextFrameTime)
    return false;

  nextFrameTime = t + std::chrono::milliseconds(frameMs);
  return this->transmitFrame(data, size);
}

void TransmitterAgent::configure(int argc, char** argv) {
  for (int i=1; i<argc; i++) {
    if (strcmp(argv[i], "-v") == 0 || strcmp(argv[i], "--verbose") == 0) {
      this->bVerbose=true;
      continue;
    }

    if (strcmp(argv[i], "--convert-32bit") == 0 || strcmp(argv[i], "-c") == 0) {
      this->converterRef = std::make_shared<Converter16to32bit>();
      continue;
    }

    if(argc <= (i+1)) {
      std::cerr << "Didn't get value for " << argv[i] << std::endl;
      break;
    }

    if(strcmp(argv[i], "--depth-port") == 0 || strcmp(argv[i], "-d") == 0) {
      this->depthPort = atoi(argv[i+1]);
      i++;
      continue;
    }

    if(strcmp(argv[i], "--fps") == 0 || strcmp(argv[i], "-f") == 0) {
      this->frameMs = 1.0f / atoi(argv[i+1]) * 1000.0f;
      i++;
      continue;
    }
  }
}

bool TransmitterAgent::transmitFrame(const void* data, size_t size) {
  // convert?
  if(converterRef) {
    if(converterRef->convert(data, size)) {
      if(bVerbose) std::cout << "converting from 16-bit to 32-bit" << std::endl;
      data = converterRef->getData();
      size = converterRef->getSize();
      // std::cout << "converted to 32bit, size: " << size << std::endl;
    } else {
      std::cerr << "16 to 32 bit conversion failed" << std::endl;
      return false;
    }
  }

  if (bCompress) {
    if(!compressor->compress(data, size)) {
      std::cerr << "compression failed" << std::endl;
      return false;
    }

    data = compressor->getData();
    size = compressor->getSize();
  }

  if (!transmitter->transmit((const char*)data, size)) {
    if(bVerbose) std::cout << "transmit of " << size << "-byte compressed depth frame FAILED (probably no connection)" << std::endl;
    return false;
  } 
  
  if(bVerbose) std::cout << "transmitted " << size << "-byte compressed depth frame" << std::endl;

  return true;
}