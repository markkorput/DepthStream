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

#include <string.h>

#include <chrono>
#include "DepthStream/Inflater.h"
#include "DepthStream/compression.h"

#define DEFAULT_BUF_SIZE (1280*720*4)

using namespace std;
using namespace depth;

Inflater::Inflater(size_t initialBufferSize) {
  this->decompressed = compression::growBuffer(NULL, 0, initialBufferSize, false /* don't free anything */);
}

void Inflater::destroy() {
  if(decompressed){
    compression::freeBuffer(decompressed);
    decompressed = NULL;
    currentBufferSize=0;
  }
}

bool Inflater::inflate(const void* data, size_t size) {
  this->inflateSize = compression::inflate(data, size, this->decompressed, this->currentBufferSize);
  return this->inflateSize != 0;
}

