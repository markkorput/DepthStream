#include "DepthStream/compression.h"

#include <memory>
#ifdef _WIN32
	#include <io.h>

	//#include "zlibdll/include/zlib.h"
	// #pragma comment(lib, "zlibdll/lib/zdll.lib")
	#include "zlib.h"
#else
	#include <unistd.h>
	#include "zlib.h"
#endif
#include <stdio.h>
#include <string.h>
#include <iostream>
#include <sstream> // std::stringstream


using namespace std;

void* depth::compression::growBuffer(void* buffer, size_t currentsize, size_t newsize, bool freeOldBuffer) {
  char* tmp = (char *) calloc( sizeof(char), newsize);

	if (tmp && buffer) {
  	memcpy(tmp, (char*)buffer, currentsize);
    if (freeOldBuffer) free(buffer);
  }

  return tmp;
}

void depth::compression::freeBuffer(void* buffer){
  free(buffer);
}

size_t depth::compression::deflate(const void* data, size_t size, void* &out, size_t &out_size, bool growTarget, size_t grow_size, bool bVerbose) {
  // verify that we got any input data
  if (data == NULL || size == 0) {
      std::cout << "nothing to deflate" << std::endl;
      return 0;
  }

  // make sure there's space in the initial target buffer
  if (!out || out_size == 0) {
    if (!growTarget) {
      std::cerr << "Buffer too small for deflation" << std::endl;
      return 0;
    }

    if(bVerbose) std::cout << "Allocating deflation buffer to " << grow_size << " bytes" << std::endl;
    out = growBuffer(out, 0, grow_size);

    if (!out) {
      std::cerr << "Could not allocate deflate buffer" << std::endl;
      return 0;
    }

    out_size = grow_size;
  }

  // zlib struct
  z_stream strm;
  strm.zalloc = Z_NULL;
  strm.zfree = Z_NULL;
  strm.opaque = Z_NULL;
  // setup "a" as the input and "b" as the compressed output
  strm.avail_in = (uInt)size; // size of input, string + terminator
  strm.next_in = (Bytef *)data; // input char array
  strm.avail_out = (uInt)out_size; // size of output
  strm.next_out = (Bytef *)out; // output char array

  // // the actual compression work.
  deflateInit(&strm, Z_BEST_SPEED);

  while (true) {

    // if our output buffer is too small
    if (strm.total_out >= out_size) {

      if (!growTarget) {
        std::cerr << "Buffer too small for inflation" << std::endl;
        return 0;
      }

      size_t newsize = strm.total_out+grow_size;
      out = growBuffer(out, out_size, newsize);

      if (!out) {
        std::cerr << "could not grow buffer" << std::endl;
        return 0;
      }

      out_size = newsize;
    }

    strm.next_out = (Bytef *) (((char*)out) + strm.total_out);
    strm.avail_out = out_size - strm.total_out;

    // deflate another chunk
    // int err = ::inflate (& strm, Z_SYNC_FLUSH);
    auto err = ::deflate(&strm, Z_FINISH);
      
    if (err == Z_STREAM_END) {
      break;
      // this->cout() << "inflated packet to: " << strm.total_out << " bytes" << std::endl;
    }
    
    if (err != Z_OK)  {
      if(bVerbose) std::cout << "inflate failed; unknown error" << std::endl;
      break;
    }
  }

  // deflateEnd(&strm);

  if (deflateEnd (& strm) != Z_OK) {
      std::cout << "deflate ended with non-OK result" << std::endl;
      // failCount++;
      return 0;
  }

  // if(deflateResult != Z_STREAM_END) {
  //   std::cout << "couldn't finish deflation" << std::endl;
  //   return 0;
  // }

  return strm.total_out;
}

size_t depth::compression::inflate(const void* compressedData, size_t compressedSize, void* &target, size_t &target_size, bool growTarget, size_t grow_size, bool bVerbose) {
  // verify that we got any input data
  if (compressedData == NULL || compressedSize == 0) {
      std::cout << "[depth::Inflater] compressedSize = 0, nothing to decompress" << std::endl;
      return false;
  }

  // make sure there's space in the initial target buffer
  if (!target || target_size == 0) {
    if (!growTarget) {
      std::cerr << "Buffer too small for inflation" << std::endl;
      return 0;
    }

    if(bVerbose) std::cout << "Allocating inflation buffer to " << grow_size << " bytes" << std::endl;
    target = growBuffer(target, 0, grow_size);

    if (!target) {
      std::cerr << "Could not grow buffer" << std::endl;
      return 0;
    }

    target_size = grow_size;
  }

  z_stream strm;
  strm.next_in = (Bytef *)compressedData;
  strm.avail_in = compressedSize;
  strm.total_out = 0;
  strm.zalloc = Z_NULL;
  strm.zfree = Z_NULL;
  strm.opaque = Z_NULL;

  if (inflateInit(&strm) != Z_OK) {
      std::cerr << "[depth::Inflater] inflator init failed" << std::endl;
      return 0;
  }

  while (true) {

    // if our output buffer is too small
    if (strm.total_out >= target_size) {

      if (!growTarget) {
        std::cerr << "Buffer too small for inflation" << std::endl;
        return 0;
      }

      size_t newsize = strm.total_out+grow_size;
      target = growBuffer(target, target_size, newsize);

      if (!target) {
        std::cerr << "could not grow buffer" << std::endl;
        return 0;
      }

      target_size = newsize;
    }

    strm.next_out = (Bytef *) (((char*)target) + strm.total_out);
    strm.avail_out = target_size - strm.total_out;

    // inflate another chunk
    int err = ::inflate (& strm, Z_SYNC_FLUSH);
    
    if (err == Z_STREAM_END) {
      break;
      // this->cout() << "inflated packet to: " << strm.total_out << " bytes" << std::endl;
    }
    
    if (err != Z_OK)  {
      if(bVerbose) std::cout << "inflate failed; unknown error" << std::endl;
      break;
    }
  }

  if (inflateEnd (& strm) != Z_OK) {
      std::cout << "inflate ended with non-OK result" << std::endl;
      // failCount++;
      return 0;
  }

  // this->cout() << "inflated to total: " << strm.total_out << " bytes" << std::endl;
  // inflateSize = strm.total_out;
  // return decompressed;
  return strm.total_out;
}

discover::middleware::packet::ConvertFunc depth::compression::middleware::compress(CompressBuffer& buffer) {
  return [&buffer](discover::middleware::packet::Packet& packet) -> discover::middleware::packet::Packet* {
    // cout << "compress " << packet.size << " bytes" << endl;
    // void* target = NULL;
    // size_t size = 0;
    size_t delflatedSize = depth::compression::deflate(packet.data, packet.size, buffer.data, buffer.size);

    if (delflatedSize == 0) return NULL;
    packet.data = buffer.data;
    packet.size = buffer.size;
    return &packet;
  };
}

discover::middleware::packet::ConvertFunc depth::compression::middleware::compress() {
  std::shared_ptr<depth::compression::CompressBuffer> compressbufferRef;  

  return [compressbufferRef](discover::middleware::packet::Packet& packet) -> discover::middleware::packet::Packet* {
    // cout << "compress " << packet.size << " bytes" << endl;
    size_t delflatedSize = depth::compression::deflate(packet.data, packet.size, compressbufferRef->data, compressbufferRef->size);

    if (delflatedSize == 0) return NULL;
    packet.data = compressbufferRef->data;
    packet.size = compressbufferRef->size;
    return &packet;
  };
}