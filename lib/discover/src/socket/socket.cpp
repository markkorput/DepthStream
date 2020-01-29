#include <sstream>
#include <iostream>
#include <discover/socket.h>
#include <memory>
#include <thread>
#ifdef _WIN32
	#pragma comment(lib, "ws2_32.lib")
	#include "Windowsstuff.h"
#else
	#include <unistd.h>
	#include <netinet/in.h>
	#include <arpa/inet.h> //inet_addr
	#include <netdb.h> //hostent
#endif

using namespace std;
using namespace discover::socket;

bool transmit(int socket, const void* data, size_t size){
   #ifdef _WIN32
	auto n = send(socket, (char*)data, size, 0);
  #else
	auto n = write(socket, data, size);
  #endif

  if(n >= 0) return true;
  #ifdef _WIN32
    closesocket(socket);
  #else
    close(socket);
  #endif

  return false;  
}

void discover::socket::packet::send(std::vector<connect::ConsumerInfo>& consumers, const void* data, size_t size) {
  char header[4];
  header[0] = (char)((size >> 24) & 0xff);
  header[1] = (char)((size >> 16) & 0xff);
  header[2] = (char)((size >> 8) & 0xff);
  header[3] = (char)(size & 0xff);

  for (auto& c : consumers) {
    if (!c.connected) continue;
    // transmit header and body, header is a 4 byte body size integer
    if (transmit(c.socket, header, 4) && transmit(c.socket, data, size)) continue;
    c.connected = false;
  }
}

int bind(int port) {
  #ifdef _WIN32
    makeSureWindowSocketsAreInitialized();
  #endif

  struct sockaddr_in serv_addr;
  int sockfd = ::socket(AF_INET, SOCK_STREAM, 0);

  int option=1;
  setsockopt(sockfd, SOL_SOCKET, SO_REUSEADDR, (char*)&option, sizeof(option));

  if (sockfd < 0) {
    #ifdef _WIN32
        std::cerr << "ERROR opening socket, WSAGetLastError gives: " << WSAGetLastError() << std::endl;
    #else
        std::cerr << "ERROR opening socket" << endl;
    #endif
     return 0;
   }

  // bzero((char *) &serv_addr, sizeof(serv_addr));
  memset((char *)&serv_addr, 0, sizeof(serv_addr));

  serv_addr.sin_family = AF_INET;
  serv_addr.sin_addr.s_addr = INADDR_ANY;
  serv_addr.sin_port = htons(port);
  if (::bind(sockfd, (struct sockaddr *) &serv_addr, sizeof(serv_addr)) < 0) {
     std::cerr << "ERROR on binding" << endl;
     return 0;
   }

  listen(sockfd, 5);
  return sockfd;
}

server::Handle discover::socket::server::create(int port, ConsumerInfoCallback callback) {

  auto ref = std::make_shared<server::ServerInfo>();
  ref->thread = new std::thread([ref, port, callback](){
    struct sockaddr_in cli_addr;
    int clientsocket;

    while(ref->active) {
      if (ref->socket == 0) {
        ref->socket = bind(port);
        // if (ref->socket == 0) {
      }
      
      if (ref->socket) {
        #ifdef _WIN32
          int clilen = sizeof(cli_addr);
        #else
          socklen_t clilen = sizeof(cli_addr);
        #endif

        clientsocket = accept(ref->socket, (struct sockaddr *) &cli_addr, &clilen);

        if (clientsocket < 0) {
          cerr << "ERROR on accept" << endl;
        } else {
          cout << "client connected" << endl;
        }

        if (callback) callback(clientsocket);
      }

      usleep(200);  
    }
  });

  return ref;
}


//     if(bBound) {
//       if(!bConnected) {
// #ifdef _WIN32
//         int clilen = sizeof(cli_addr);
// #else
//         socklen_t clilen = sizeof(cli_addr);
// #endif
//         clientsocket = accept(sockfd, (struct sockaddr *) &cli_addr, &clilen);
//         if (clientsocket < 0) {
//           error("ERROR on accept");
//           bConnected=false;
//         } else {
//           std::cout << "client connected" << std::endl;
//           bConnected=true;
//         }
//       }
