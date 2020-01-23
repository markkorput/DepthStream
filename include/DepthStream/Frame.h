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
#include <functional>

namespace depth {

  class ReadOnlyFrame;
  typedef ReadOnlyFrame Frame;
  typedef std::shared_ptr<Frame> FrameRef;

  class WritableFrame;
  typedef std::shared_ptr<WritableFrame> WritableFrameRef;

  /**
   * \brief A read-only wrapper around a data block of a specified size
   *
   * Frame is a read-only wrapper around a data block pointer and a size attribute.
   * Frames can be initialized both with _owned_ data (which will be
   * deallocated by the Frame's destructor) as well as with _external_ data (which
   * will simply be abandoned by the Frame's destructor).
   */
  class ReadOnlyFrame {

    public: // static methods

      typedef std::function<void(const void*, size_t)> InputFunc;

      /// Allocates a Frame for _owned_ data
      static FrameRef ref(size_t size) { return std::make_shared<Frame>(size); }

      /// Initializes a frame with _external_ data
      static FrameRef refToExternalData(const void* data, size_t size) {
        return std::make_shared<Frame>(data, size);
      }

      /// Initializes a Frame with _owned_ data ("adopts" the provided data)
      static FrameRef refWithData(void* data, size_t size) {
        auto ref = std::make_shared<Frame>();
        ref->_size = size;
        ref->ownedData = data;
        return ref;
      }

    public: // constructors

      /// Create an uninitialized, empty Frame instance
      ReadOnlyFrame(){}

      /// Allocates size bytes of _owned_ data
      ReadOnlyFrame(size_t size) : _size(size) { ownedData = malloc(size); }

      /// Initializes with _external_ data
      ReadOnlyFrame(const void* data, size_t size) : _size(size), externalData(data) { }

      /// Deallocates _owned_ data (if any)
      ~ReadOnlyFrame() { if(ownedData) free(ownedData); }

    public: // getters

      /// @return A pointer to the frame's data block (either owned or external), can be NULL
      const void* data() const { return ownedData ? ownedData : externalData; }

      /// @return The size of the frame's data block (either owned or external) in bytes (can be zero)
      size_t size() const { return _size; }

      /**
       * \brief execute the given function with the frame's current content
       *
       * This template function fascilitates specifically towards functional programming and
       * lets the owner "convert" the data by choosing a return-value type.
       * When converting to another Frame reference, linked notation is possible;
       * frame->convert<FrameRef>(scaleDown)->convert<FrameRef>(blackAndWhite)->convert<FrameRef>(blur)
       */
      template<typename ResultType>
      ResultType convert(std::function<ResultType(const void*,size_t)> func) const {
        return func(data(), size());
      }

    protected: // attributes

      size_t _size=0;
      void* ownedData=NULL;
      const void* externalData=NULL;
  };

  class WritableFrame : public Frame {

    public:

      static WritableFrameRef ref(size_t size) { return std::make_shared<WritableFrame>(size); }

      static WritableFrameRef concatRef(const Frame &f1, const Frame &f2) {
        auto r = WritableFrame::ref(f1.size() + f2.size());
        r->concat(f1, f2);
        return r;
      }

    public:

      /// Allocates size bytes of _owned_ data
      WritableFrame(size_t size) : ReadOnlyFrame(size) {}

      void write(const void* data, size_t size) {
        this->write(data, size, 0);
      }

      void write(const void* data, size_t size, size_t offset) {
        memcpy((void*)((char*)this->ownedData + offset), data, size);
      }

      void concat(const Frame &f1, const Frame &f2);

      void* buffer() { return this->ownedData; }
  };
}
