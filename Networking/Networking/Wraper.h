#pragma once
#include "PluginSettings.h"
#include "IncludeThis.h"

//Updated: 2020/05/06 - this is after the school year

/*When transfering to c#:
* Added   enums EventsPoll & REventsPoll: check
* Added   setBlocking function: check
* Added   pollEvents function:
* Edited  createIPEndpointData function: check
* Edited  createSocketData function: check
* Edited  initSocket function: check
* Updated SocketOptions enum: check
* Updated SocketData struct: check
* Changed initIPEndpoint to createIPEndpointData: check
* Changed createSocket to initSocket: check
* Changed connectEndpoint To connectEndpointToSocket: check
*/

typedef const char* cstring;
#ifdef __cplusplus
extern "C"
{
#endif
	//ERROR//

	//gets the last error that happened
	PLUGIN_API cstring getLastNetworkError(int);

	//NETWORK//

	//initializes Winsock
	PLUGIN_API bool initNetwork(int);
	//Shutdown Winsoc
	PLUGIN_API bool shutdownNetwork(int);


	//ENDPOINT//

	//Creates a new IPEndpoint handle for a given IP and port. Multiple IPEndpoints can be created
	PLUGIN_API IPEndpointData createIPEndpointData(const char* ip, unsigned short port, IPVersion ipv = IPVersion::IPv4);


	//SOCKET//

	//Creates a new Socket handle for manageing IPEndpoints. 
	PLUGIN_API SocketData createSocketData(IPVersion = IPVersion::IPv4);

	//initializes the socket to use UDP or TCP conection types with the option of blocking sockets
	PLUGIN_API PResult initSocket(SocketData&, SocketType = TCP, bool blocking = true);
	//closes socket so it can not be used unless initSocket() is called one again.
	PLUGIN_API PResult closeSocket(SocketData&);
	//sets a socket to be either blocking or non-blocking. Must be called after socket is created
	PLUGIN_API PResult setBlocking(SocketData&, bool blocking = true);
	//polls and sets revents for non-blocking sockets
	PLUGIN_API PResult pollEvents(SocketData& soc, int delayInMili, short eventflags = 0);

	//Bind Endpoint to socket.
	PLUGIN_API 	PResult bindEndpointToSocket(const IPEndpointData& ep, const SocketData&);
	//Listens for endpoint connection to the socket. It will block until a new connection is found. 
	PLUGIN_API 	PResult listenEndpointToSocket(const IPEndpointData& ep, const SocketData&, int backlog = 5);
	//Attempts to accept the listened connection. 
	PLUGIN_API 	PResult acceptSocket(const SocketData& in, SocketData& out,IPEndpointData& outip);
	//Connects endpoint to socket
	PLUGIN_API 	PResult connectEndpointToSocket(const IPEndpointData& ep, const  SocketData&);


	//Sending Data//

	// TCP

	//Send packet over TCP server. Not guaranteed to send all bytes.
	PLUGIN_API PResult sendPacketData(const SocketData&, void* data, int numberOfBytes, int& bytesSent);
	//Receive packet over TCP server. Not guaranteed to recieve all bytes.
	PLUGIN_API PResult recvPacketData(const SocketData&, void* dest, int numberOfBytes, int& bytesRecv);
	//Send entire packet over TCP server. guaranteed to send all bytes.
	PLUGIN_API PResult sendAllPacketData(const SocketData&, void* data, int numberOfBytes);
	//Receive entire packet over TCP server. guaranteed to recieve all bytes.
	PLUGIN_API PResult recvAllPacketData(const SocketData&, void* dest, int numberOfBytes);

	//UDP

	//Receive packet over UDP server. Not guaranteed to recieve all bytes.
	PLUGIN_API PResult recvFromPacketData(const SocketData&, void* data, int numberOfBytes, int& bytesSent, IPEndpointData& ep);
	//Send packet over UDP server. Not guaranteed to send all bytes.
	PLUGIN_API PResult sendToPacketData(const SocketData&, void* data, int numberOfBytes, int& bytesSent, const IPEndpointData& ep);

#ifdef __cplusplus
}
#endif

