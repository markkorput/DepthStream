#include <iostream>
#include <discover/osc/osc.h>

using namespace std;

// https://stackoverflow.com/questions/2616011/easy-way-to-parse-a-url-in-c-cross-platform
discover::osc::urlinfo::urlinfo(const string& url_s) {
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