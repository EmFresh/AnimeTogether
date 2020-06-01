#pragma once
#define WIN32_LEAN_AND_MEAN

#include <WinSock2.h>
#include "LastNetworkError.h"
#include "IPVersion.h"
#include "IPEndpoint.h"
#include "Packet.h"


enum SocketOption
{
	TCP_NoDelay,
	IPV6_Only,
};

enum SocketType
{
	TCP,
	UDP
};

enum PResult
{
	P_Success,
	P_UnknownError
};

enum  EventsPoll
{
	EP_RDBAND = POLLRDBAND,//Priority band (out-of-band) data may be read without blocking.
	EP_RDNORM = POLLRDNORM,//Normal data may be read without blocking.
	EP_WRNORM = POLLWRNORM,//Normal data may be written without blocking.
	EP_IN = POLLIN,        //combination of the EPRDNORM and EPRDBAND
	EP_OUT = POLLOUT,      //same as the EPWRNORM
};

enum REventsPoll
{
	REP_ERR = POLLERR,      //An error has occurred.
	REP_NVAL = POLLNVAL,	   //An invalid socket was used.
	REP_HUP = POLLHUP,	   //A stream-oriented connection was either disconnected or aborted.
	REP_RDBAND = POLLRDBAND,//Priority band (out-of-band) data may be read without blocking.
	REP_RDNORM = POLLRDNORM,//Normal data may be read without blocking.
	REP_WRNORM = POLLWRNORM,//Normal data may be written without blocking.
	REP_IN = POLLIN,         //combination of the REPRDNORM and REPRDBAND
	REP_OUT = POLLOUT,       //same as the REPWRNORM
};

typedef SOCKET SocketHandle;
typedef SHORT REVENTS;

//updated socket data
struct SocketData
{
	IPVersion IPv;
	SocketHandle hnd = INVALID_SOCKET;
	u_int pollCount = 0;
	REVENTS revents = 0;
};

class Socket
{
public:
	static SocketData createSocketData(IPVersion    ipv = IPVersion::IPv4,
									   SocketHandle hnd = INVALID_SOCKET);

	static PResult init(SocketData&, SocketType = SocketType::TCP, bool blocking = true);
	static PResult close(SocketData&);

	static PResult setBlocking(SocketData&, bool blocking = true);
	static PResult pollEvents(SocketData&, int delay, short evntflags = 0);

	static PResult bindEndpoint(const IPEndpointData& ep, const SocketData&);
	static PResult listenEndpoint(const IPEndpointData& ep, const  SocketData&, bool v6only = false, int backlog = 5);
	static PResult acceptSocket(const SocketData&, SocketData&, IPEndpointData& = useless);
	static PResult connectEndpoint(const IPEndpointData& ep, const SocketData&);

	// Sending Data //
	//TCP
	static PResult sendPacket(const SocketData&, void* data, int numberOfBytes, int& bytesSent);
	static PResult sendPacket(const SocketData&, Packet& data);
	static PResult recvPacket(const SocketData&, void* dest, int numberOfBytes, int& bytesRecv);
	static PResult recvPacket(const SocketData&, Packet& dest);
	static PResult sendAllPacket(const SocketData&, void* data, int numberOfBytes);
	static PResult recvAllPacket(const SocketData&, void* dest, int numberOfBytes);

	//UDP
	static PResult recvFromPacket(const SocketData&, void* data, int numberOfBytes, int& bytesSent, IPEndpointData& ep);
	static PResult recvFromPacket(const SocketData&, void* data, int numberOfBytes, int& bytesSent);
	static PResult recvFromPacket(const SocketData&, void* data, int numberOfBytes, IPEndpointData& ep);
	static PResult recvFromPacket(const SocketData&, void* data, int numberOfBytes);
	static PResult sendToPacket(const SocketData&, const void* data, int numberOfBytes, int& bytesSent, const IPEndpointData& ep);
	static PResult sendToPacket(const SocketData&, const void* data, int numberOfBytes, const IPEndpointData& ep);

private:
	static PResult initTCP(SocketData& data, bool block);
	static PResult initUDP(SocketData& data, bool block);
	static PResult setSocketOption(const SocketData&, SocketOption opt, void* val);
	static IPEndpointData useless;


};