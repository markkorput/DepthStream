#include <iostream>
#include <discover/osc/osc.h>
// #include <lo/lo.h>
#include <lo/lo_cpp.h>
#include <memory>

using namespace std;
using namespace discover::osc;

void createMessageList(vector<lo::Message>& messages, const void* data, size_t size, size_t maxMessageSize) {
  size_t done = 0;
  size_t perMessage = maxMessageSize;

  int i = 0;
  while (done < size) {    
    size_t cursize = perMessage > (size - done) ? (size - done) : perMessage;

    lo::Message m;
    m.add("i", size); // total size
    m.add("i", done); // offset
    m.add(lo::Blob(cursize, cursize + (char*)data));
    messages.push_back(m);

    done += cursize;
  }
}

void discover::osc::packet::send(const std::vector<connect::ConsumerInfo>& consumers, const void* data, size_t size, const std::string& messageAddr) {
  // cout << "Sending "<<messageAddr<<" packet to " << consumers.size() << " consumers" << endl;

  vector<lo::Message> messages;

  auto max = 4096;
  if (size > max) {
    createMessageList(messages, data, size, max);
  } else {
    lo::Message m;
    m.add(lo::Blob(size, data));
    messages.push_back(m);
  }

  for (auto& c : consumers) {
    // cout << "PacketSender::submit sending to "<<size<<" bytes to " << c.host << ":" << c.port << " over " << messages.size() << " messages" << endl;
    auto a = lo::Address(c.host, c.port);

    for (auto& m : messages) {
      auto res = a.send(messageAddr.c_str(), m);

      if (res < 0) {
        cout << "Failed to send " << messageAddr << " OSC packet ("<< size <<" bytes) to " << c.host << ":" << c.port << endl;
        break;
      }
    }
  }
}

void discover::osc::packet::addCallback(server::InstanceHandle serverHandle, DataFunc callback, const std::string& messageAddr, Buffer* buffer) {
  auto st = (lo::ServerThread*)serverHandle;

  cout << "registering OSC handler for: " << messageAddr << endl;

  st->add_method(messageAddr, "b", [callback](lo_arg **argv, int) {
    void* data = &argv[0]->blob.data;
    size_t size = argv[0]->blob.size;
    cout << "got data: " << size <<endl;
    callback(data, size);
  });

  if (buffer == NULL) return;

  // partial packet message handler
  st->add_method(messageAddr, "iib", [callback, buffer](lo_arg **argv, int) {

    int total = argv[0]->i;
    int offset = argv[1]->i;
    
    void* data = &argv[2]->blob.data;
    size_t size = argv[2]->blob.size;

    // cout << "ttl: " << total << " size: " << size << " offset: " << offset << endl;

    if (buffer->data == NULL || buffer->size < total) {
      buffer->data = growBuffer(buffer->data, buffer->size, total, true);
      buffer->size = total;
    }

    if (offset + size > buffer->size) {
      cout << "Partial message too big" << endl;
      return;
    }

    memcpy(&((char*)buffer->data)[offset], data, size);

    if (offset + size == total) {
      callback(buffer->data, total);
    }
  });
}

void* discover::osc::packet::growBuffer(void* buffer, size_t currentsize, size_t newsize, bool freeOldBuffer) {
  char* tmp = (char *) calloc( sizeof(char), newsize);

	if (tmp && buffer) {
  	memcpy(tmp, (char*)buffer, currentsize);
    if (freeOldBuffer) free(buffer);
  }

  return tmp;
}

void discover::osc::packet::freeBuffer(void* buffer){
  free(buffer);
}
