//
//  This file is part of the ofxDepthStream [https://github.com/fusefactory/ofxDepthStream]
//  Copyright (C) 2018 Fuse srl
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

#include <string>
#include <iostream>
#include <fstream>
#include <thread>
#include <chrono>
#include "Playback.h"

using namespace depth;

void Playback::start(const std::string& name) {
  filename = name;
  infile = new std::ifstream(filename, std::ofstream::binary);

  frameCount=0;
  bPlaying=true;
  startTime = std::chrono::steady_clock::now();

  std::cout << "started playback of: " << filename << std::endl;
  if (mStartCallback) this->mStartCallback();
  this->update();
}

void Playback::startThreaded() {
  this->thread = new std::thread(std::bind(&Playback::threadFunc, this));
}

void Playback::startThreaded(const std::string& name){
  filename = name;
  this->startThreaded();
}

void Playback::stop(bool wait) {
  bPlaying=false;

  if(infile){
    infile->close();
    delete infile;
    infile=NULL;
  }

  if (mStopCallback) this->mStopCallback();

  if(wait){
    if(thread) {
      thread->join();
      delete thread;
      thread=NULL;
    }
  }
}

bool Playback::update(FrameCallback inlineCallback) {
  if(!bPlaying) return false;

  if(!nextFrame){
    nextFrame=readFrame();

    if(!nextFrame){
      bPlaying=false;
      this->onEnd();
      return false;
    }
  }

  if(nextFrame) {
    auto ms = (uint32_t)std::chrono::duration_cast<std::chrono::milliseconds>(std::chrono::steady_clock::now() - startTime).count();

    if(ms > nextFrame->time) {
      if(inlineCallback)
        inlineCallback(nextFrame->buffer, nextFrame->size);

      // implement/execute our Buffer interface
      Buffer::write(nextFrame->buffer, nextFrame->size);

      nextFrame = NULL;
      frameCount += 1;
      return true;
    }
  }

  return false;
}

Playback::Frame* Playback::readFrame() {
  if(infile->read((char*)&frame.time, sizeof(uint32_t)) &&
    infile->read((char*)&frame.size, sizeof(uint32_t)) &&
    infile->read((char*)&frame.buffer, frame.size)) {
    return &frame;
  }

  return NULL;
}

void Playback::threadFunc() {
  this->start(this->filename);
  while(this->bPlaying){
    this->update();
    std::this_thread::sleep_for(std::chrono::duration<long, std::milli>(50l));
  }
}

void Playback::onEnd() {
  stop(false);
  if(bLoop) {
    if(frameCount == 0) {
      std::cout << "[depth::Playback] file appears empty" << std::endl;
    } else {
      start(this->filename);
    }
  }
}
