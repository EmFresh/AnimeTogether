#include "Wraper.h"

cstring getLastNetworkError(int)
{
	//static std::string tmp;//NEEDED DO NOT REMOVE	
	return LastNetworkError::GetLastError().c_str();
}

bool initNetwork(int)
{
	return Network::init();
}

bool shutdownNetwork(int)
{
	return Network::shutdown();
}

IPEndpointData createIPEndpointData(const char* ip, unsigned short port, IPVersion ipv)
{
	return IPEndpoint::createIPEndpoint(ip, port, ipv);
}

SocketData createSocketData(IPVersion ipv)
{
	return Socket::createSocketData(ipv);
}

PResult initSocket(SocketData& sock, SocketType type, bool blocking)
{
	return Socket::init(sock, type, blocking);
}

PResult closeSocket(SocketData& soc)
{
	return Socket::close(soc);
}

PResult setBlocking(SocketData& soc, bool blocking)
{
	return Socket::setBlocking(soc, blocking);
}

PResult pollEvents(SocketData& soc, int delayInMili, short eventflags)
{
	return Socket::pollEvents(soc, delayInMili, eventflags);
}

PResult bindEndpointToSocket(const IPEndpointData& ep, const SocketData& soc)
{
	return Socket::bindEndpoint(ep, soc);
}

PResult listenEndpointToSocket(const IPEndpointData& ep, const SocketData& soc, int backlog)
{
	return Socket::listenEndpoint(ep, soc, backlog);
}

PResult acceptSocket(const SocketData& in, SocketData& out,IPEndpointData& ip)
{
	return Socket::acceptSocket(in, out,ip);
}

PResult connectEndpointToSocket(const IPEndpointData& ep, const SocketData& soc)
{
	return Socket::connectEndpoint(ep, soc);
}


PResult sendPacketData(const SocketData& soc, void* data, int numberOfBytes, int& bytesSent)
{
	return Socket::sendPacket(soc, data, numberOfBytes, bytesSent);
}

PResult recvPacketData(const SocketData& soc, void* dest, int numberOfBytes, int& bytesRecv)
{
	return Socket::recvPacket(soc, dest, numberOfBytes, bytesRecv);
}

PResult sendAllPacketData(const SocketData& soc, void* data, int numberOfBytes)
{
	return Socket::sendAllPacket(soc, data, numberOfBytes);
}

PResult recvAllPacketData(const SocketData& soc, void* dest, int numberOfBytes)
{
	return Socket::recvAllPacket(soc, dest, numberOfBytes);
}



PResult recvFromPacketData(const SocketData& soc, void* data, int numberOfBytes, int& bytesSent, IPEndpointData& ep)
{
	return Socket::recvFromPacket(soc, data, numberOfBytes, bytesSent, ep);
}


PResult sendToPacketData(const SocketData& soc, void* data, int numberOfBytes, int& bytesSent, const IPEndpointData& ep)
{
	return Socket::sendToPacket(soc, data, numberOfBytes, bytesSent, ep);
}
