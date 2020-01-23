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

#include <iostream>
#include "zlib.h"
#include "DepthStream/Compressor.h"
#include "DepthStream/compression.h"

using namespace depth;

bool Compressor::compress(const void* data, size_t size) {
  // memcpy(buffer, data, size);
  if(data){
    this->size = size;
    this->buffer = (const unsigned char*)data;

    // std::string ss(data, size);
    // std::cout << "compressing: " << ss << std::endl;
    // return doCompression();
    compression::deflate(this->buffer, this->size, this->compressed, BUF_SIZE);
  }

  std::cout << "Got NULL pointer to compress" << std::endl;
  return false;
}