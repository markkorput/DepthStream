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

void discover::osc::packet::addCallback(server::InstanceHandle serverHandle, DataFunc callback, const std::string& messageAddr) {
  auto st = (lo::ServerThread*)serverHandle;

  cout << "registering OSC handler for: " << messageAddr << endl;

  st->add_method(messageAddr, "b", [callback](lo_arg **argv, int) {
    void* data = &argv[0]->blob.data;
    size_t size = argv[0]->blob.size;
    cout << "got data: " << size <<endl;
    callback(data, size);
  });

  // partial packet message handler
  st->add_method(messageAddr, "iib", [callback](lo_arg **argv, int) {
    // cout << "Got partial msg" <<endl;
    static struct {
      int offset = 0;
      int total = 0;
      void* buffer=NULL;
    } info; // is this safe?

    int ttl = argv[0]->i;
    int offset = argv[1]->i;

    void* data = &argv[2]->blob.data;
    size_t size = argv[2]->blob.size;


    if (info.buffer == NULL) {
      if (offset == 0 && ttl > 0) {
          // cout << "starting new partial msg" <<endl;
          info.buffer = calloc( sizeof(char), ttl);
          info.offset = 0;
          info.total = ttl;
      } else {
        cout << "invalid partial message; offset=" << offset <<", ttl=" << ttl << endl;
        return;
      }
    }

    if (info.buffer == NULL) return;

    if (info.offset != offset || info.total != ttl || info.offset+size > info.total) {
      // cout << "error during partial msg" <<endl;
      free(info.buffer);
      info.buffer = NULL;
      return;
    }

    memcpy(info.offset+(char*)info.buffer, data, size);
    info.offset += size;
    // cout << "added " << size  << " bytes to partial message" << endl;

    if (info.offset == ttl) {
      // cout << "finished partial msg" <<endl;
      callback(info.buffer, ttl);
      free(info.buffer);
      info.buffer = NULL;
    }
  });
}