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

#pragma once

#include <memory>
#include "DepthStream/Frame.h"

namespace depth {
  class Compressor;
  typedef std::shared_ptr<Compressor> CompressorRef;

  /**
   * \brief The Compressor class uses the compression::deflate method and provides
   * a re-usable buffer for the compressed data to avoid new memory allocation for every frame.
   */
  class Compressor {
    public:

      ~Compressor();

      /**
       * \brief The compress method invokes the compression::inflate method using the given data,
       * and the internal re-usable buffer to hold the compressed data.
       */
      bool compress(const void* data, size_t size);


      /**
       * \brief getData returns a pointer to the internal data buffer
       * that holds the data of the last compress action
       * returns (NULL if there are no previous compression actions).
       */
      inline const void* getData() { return buffer; }

      /**
       * \brief returns the size of the data that resulted from
       * the last compress action. Returns 0 (zero) if there hasn't been
       * any compress actions yet, or if the last compress action failed.
       */
      inline int getSize(){ return dataSize; }

    private:
      void* buffer = NULL;
      size_t buffer_size = 0;
      unsigned int dataSize = 0;
  };
}
