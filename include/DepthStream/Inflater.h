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

namespace depth {
  class Inflater;
  typedef std::shared_ptr<Inflater> InflaterRef;

  /**
   * \brief Inflates ("decompresses") a package ("frame") of data compressed using zlib doCompression
   */
  class Inflater {
    public:

      Inflater() {}
      Inflater(std::size_t initialBufferSize);

      /// Deallocates the memory used to store inflated content (will not deallocate the memory that was release using the releaseData method)
      ~Inflater(){
        destroy();
      }

      /// Deallocates the memory used to store inflated content (will not deallocate the memory that was release using the releaseData method)
      void destroy();

      /// Performs decompression on the provided data package
      bool inflate(const void* data, std::size_t size);

      /// Returns the inflated size of the last inflate operation
      std::size_t getSize() const { return inflateSize; }

      /// Returns a pointer to the inflated package data (will be NULL when no inflation is performed or after releaseData is called)
      const void* getData() const { return decompressed; }

      /// "Releases" (abandons) and returns a pointer to the allocated data for decompression.
      /// When using this method, the caller is responsible for deallocating the returned memory block
      void* releaseData(){ void* tmp = decompressed; decompressed=NULL; return tmp; }

      void setVerbose(bool verbose) { bVerbose = verbose; }

    private:
      void* decompressed=NULL;
      std::size_t currentBufferSize=0;
      std::size_t inflateSize=0;
      bool bVerbose=false;
  };
}
