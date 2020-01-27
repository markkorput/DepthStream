#pragma once
#include <stdlib.h>
#include <string>


namespace discover { namespace osc {
  void broadcast_service(const std::string& serviceId, int port, const std::string& url);
}}