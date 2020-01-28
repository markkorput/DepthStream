#include <iostream>
#include <discover/osc/osc.h>
// #include <lo/lo.h>
#include <lo/lo_cpp.h>

using namespace std;

// https://stackoverflow.com/questions/2616011/easy-way-to-parse-a-url-in-c-cross-platform
struct urlinfo {
  std::string protocol, host, port, path, query;

  urlinfo(const string& url_s) {
    // protocol
    const string prot_end("://");
    string::const_iterator prot_i = search(url_s.begin(), url_s.end(),
                                           prot_end.begin(), prot_end.end());
    protocol.reserve(distance(url_s.begin(), prot_i));
    transform(url_s.begin(), prot_i,
              back_inserter(protocol),
              [](char c) -> char { return std::tolower(c); }); // icase
    if( prot_i == url_s.end() )
        return;

    // host
    advance(prot_i, prot_end.length());
    string::const_iterator path_i = find(prot_i, url_s.end(), '/');
    host.reserve(distance(prot_i, path_i));
    transform(prot_i, path_i,
              back_inserter(host),
              [](char c) -> char { return std::tolower(c); }); // icase

    // path
    string::const_iterator query_i = find(path_i, url_s.end(), '?');
    path.assign(path_i, query_i);
    
    // query
    if( query_i != url_s.end() )
        ++query_i;
    query.assign(query_i, url_s.end());

    // port
    auto pos = host.find(':');
    if (pos != string::npos) {
      port = host.substr(pos+1);
      host = host.substr(0, pos);
    }
  }
};

void discover::osc::broadcast::announce(const string& serviceId, const vector<int>& ports, const string& url) {
  string addr="/discover/broadcast/"+serviceId;

  for (auto port : ports) {
    lo::Address a("255.255.255.255", port); 
    a.send(addr.c_str(), "s", url.c_str());
  }
}

void discover::osc::broadcast::add_service_found_callback(server::InstanceHandle server, const std::string& serviceId, ServiceInfoFunc callback) {
  string addr="/discover/broadcast/"+serviceId;

  auto st = (lo::ServerThread*)server;

  printf("registering OSC handler for: %s\n", addr.c_str());
  st->add_method(addr, "s", [callback](lo_arg **argv, int) {
    std::string url = &argv[0]->s; // ie. "osc.udp://markbookproretina.local:4445/"
    // cout << "Received service broadcast: " << url << endl;
    urlinfo info(url);
    // cout << "protocol=" <<info.protocol<<" host="<<info.host<<" port="<<info.port<<endl;
    callback(info.host, stoi(info.port));
  });
}