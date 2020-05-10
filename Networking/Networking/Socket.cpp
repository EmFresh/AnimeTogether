#include "Socket.h"
#include <cassert>

typedef const char* cstring;
IPEndpointData Socket::useless = IPEndpointData();


SocketData Socket::createSocketData(IPVersion version, SocketHandle hnd)
{
	SocketData data;
	if(!(version == IPVersion::IPv4 || version == IPVersion::IPv6))
	{
		LastNetworkError::SetLastError("create socket data error: invalid IP version");
		return data;
	}

	data.hnd = hnd;
	data.IPv = version;

	return data;
}

PResult Socket::init(SocketData& soc, SocketType in, bool block)
{

	switch(in)
	{
	case TCP:
		return initTCP(soc, block);
	case UDP:
		return initUDP(soc, block);
	default:
		LastNetworkError::SetLastError("init socket error: unknown socket type");
		return PResult::P_UnknownError;
	}
}

PResult Socket::close(SocketData& soc)
{

	if(soc.hnd == INVALID_SOCKET)
	{
		LastNetworkError::SetLastError("close error: invalid handle");
		return P_UnknownError;
	}

	if(!soc.hnd)
	{
		LastNetworkError::SetLastError("close error: hnd not initalized");
		return PResult::P_UnknownError;
	}


	PResult ans = PResult::P_Success;
	if(shutdown(soc.hnd, SD_BOTH))
	{
		LastNetworkError::SetLastError("socket close shutdown error: ", WSAGetLastError());
		ans = PResult::P_UnknownError;
	}


	if(closesocket(soc.hnd))
	{
		if(ans == PResult::P_UnknownError)
			LastNetworkError::SetLastError((LastNetworkError::GetLastError()
										   + "\nsocket close error: ").c_str(), WSAGetLastError());
		else
			LastNetworkError::SetLastError("socket close error: ", WSAGetLastError());

		ans = PResult::P_UnknownError;
	}
	else
		soc.hnd = INVALID_SOCKET;

	return ans;
}


PResult Socket::setBlocking(SocketData& soc, bool block)
{
	u_long isblocking = block ? FALSE : TRUE;//this is correct
	if(!soc.hnd)
	{
		LastNetworkError::SetLastError("set blocking error: hnd not initalized");
		return PResult::P_UnknownError;
	}
	int result = ioctlsocket(soc.hnd, FIONBIO, &isblocking);

	if(result)
	{
		LastNetworkError::SetLastError("set blocking error: ", WSAGetLastError());
		return PResult::P_UnknownError;
	}
	return PResult::P_Success;
}

PResult Socket::pollEvents(SocketData& soc, int delayInMili, short eventflags)
{
	if(!soc.hnd)
	{
		LastNetworkError::SetLastError("poll events error: hnd not initalized");
		return PResult::P_UnknownError;
	}

	WSAPOLLFD poll;
	poll.fd = soc.hnd;
	poll.events = eventflags;
	poll.revents = 0;

	if((soc.pollCount = WSAPoll(&poll, 1, delayInMili)) > 0)
	{
		if(poll.revents & (REP_ERR | REP_NVAL | REP_HUP))
		{
			if(poll.revents & (REP_ERR))
				LastNetworkError::SetLastError("poll events error: An unknown error has occurred.");

			if(poll.revents & (REP_NVAL))
				LastNetworkError::SetLastError("poll events error: An invalid socket was used.");

			if(poll.revents & (REP_HUP))
				LastNetworkError::SetLastError("poll events error: A stream-oriented connection was either disconnected or aborted.");


			soc.pollCount = 0;
			soc.revents = 0;//
			return PResult::P_UnknownError;
		}
	}

	soc.revents = poll.revents;
	return PResult::P_Success;
}


PResult Socket::bindEndpoint(const IPEndpointData& ep, const SocketData& soc)
{
	if(soc.IPv != ep.IPv)
	{
		LastNetworkError::SetLastError("bind endpoint error: IP versions do not match");
		return PResult::P_UnknownError;
	}
	if(!soc.hnd)
	{
		LastNetworkError::SetLastError("bind endpoint error: hnd not initalized");
		return PResult::P_UnknownError;
	}

	int result;
	if(ep.IPv == IPVersion::IPv6)
	{
		sockaddr_in6 addr;
		addr = IPEndpoint::getSockAddrIPv6(ep);
		result = bind(soc.hnd, (sockaddr*)&addr, sizeof(sockaddr_in6));
	}
	else//IPv4
	{
		sockaddr_in addr;
		addr = IPEndpoint::getSockAddrIPv4(ep);
		result = bind(soc.hnd, (sockaddr*)&addr, sizeof(sockaddr_in));
	}

	if(result == SOCKET_ERROR)
	{
		LastNetworkError::SetLastError("bind endpoint error: ", WSAGetLastError());
		return PResult::P_UnknownError;
	}
	return PResult::P_Success;
}

PResult Socket::listenEndpoint(const IPEndpointData& ep, const SocketData& soc, bool v6Only, int backlog)
{
	if(!soc.hnd)
	{
		LastNetworkError::SetLastError("listen endpoint error: hnd not initalized");
		return PResult::P_UnknownError;
	}

	BOOL onlyV6 = v6Only;
	if(soc.IPv == IPVersion::IPv6)
		if(setSocketOption(soc, SocketOption::IPV6_Only, &onlyV6) != PResult::P_Success)
			return PResult::P_UnknownError;

	if(bindEndpoint(ep, soc) != PResult::P_Success)
		return PResult::P_UnknownError;

	int result = listen(soc.hnd, backlog);

	if(result == SOCKET_ERROR)
	{
		LastNetworkError::SetLastError("listen endpoint error: ", WSAGetLastError());

		puts("");
		return PResult::P_UnknownError;
	}


	return PResult::P_Success;
}

PResult Socket::acceptSocket(const SocketData& insoc, SocketData& outsoc,IPEndpointData& outIP)
{
	if(!(insoc.IPv == IPVersion::IPv4 || insoc.IPv == IPVersion::IPv6))
	{
		LastNetworkError::SetLastError("accept socket error: Invalid IP version");
		return PResult::P_UnknownError;
	}
	if(!insoc.hnd)
	{
		LastNetworkError::SetLastError("accept socket error: hnd not initalized");
		return PResult::P_UnknownError;
	}

	SocketHandle acceptConditionHnd = INVALID_SOCKET;
	int len;

	sockaddr* addr;
	if(insoc.IPv == IPVersion::IPv6)
	{
		static sockaddr_in6 addr2;addr2 = {};
		addr = (sockaddr*)&addr2;
		len = sizeof(sockaddr_in6);
		acceptConditionHnd = accept(insoc.hnd, (sockaddr*)&addr2, &len);//handle to other connection
	}
	else//IPv4
	{
		sockaddr_in addr2 = {};
		addr = (sockaddr*)&addr2;

		len = sizeof(sockaddr_in);
		acceptConditionHnd = accept(insoc.hnd, (sockaddr*)&addr2, &len);//handle to other connection
	}

	if(acceptConditionHnd == INVALID_SOCKET)
	{
		LastNetworkError::SetLastError("accept socket error: ", WSAGetLastError());

		return PResult::P_UnknownError;
	}

	
	outIP = IPEndpoint::createIPEndpoint(addr);
	outsoc = createSocketData(insoc.IPv, acceptConditionHnd);
	return PResult::P_Success;
}

PResult Socket::connectEndpoint(const IPEndpointData& ep, const SocketData& soc)
{
	if(soc.IPv != ep.IPv)
	{
		LastNetworkError::SetLastError("connent endpoint error: IP versions do not match");
		return PResult::P_UnknownError;
	}
	if(!soc.hnd)
	{
		LastNetworkError::SetLastError("connect endpoint error: hnd not initalized");
		return PResult::P_UnknownError;
	}
	int result;
	if(soc.IPv == IPVersion::IPv6)
	{
		sockaddr_in6 addr = IPEndpoint::getSockAddrIPv6(ep);
		result = connect(soc.hnd, (sockaddr*)&addr, sizeof(sockaddr_in6));
	}
	else
	{
		sockaddr_in addr = IPEndpoint::getSockAddrIPv4(ep);
		result = connect(soc.hnd, (sockaddr*)&addr, sizeof(sockaddr_in));
	}

	if(result)
	{
		LastNetworkError::SetLastError("connect endpoint error: ", WSAGetLastError());

		return PResult::P_UnknownError;
	}
	return PResult::P_Success;
}

PResult Socket::sendPacket(const SocketData& soc, void* data, int numberOfBytes, int& bytesSent)
{
	if(!soc.hnd)
	{
		LastNetworkError::SetLastError("send packet error: hnd not initalized");
		return PResult::P_UnknownError;
	}

	bytesSent = send(soc.hnd, (const char*)data, numberOfBytes, NULL);

	if(bytesSent == SOCKET_ERROR)
	{


		LastNetworkError::SetLastError("send packet error: ", WSAGetLastError());

		return PResult::P_UnknownError;
	}
	return PResult::P_Success;
}

PResult Socket::sendPacket(const SocketData& soc, Packet& data)
{
	uint16_t encodedPacketSize = (uint16_t)htonl((u_long)data.buffer.size());
	PResult result = sendAllPacket(soc, &encodedPacketSize, sizeof(uint16_t));

	if(result != PResult::P_Success)
		return PResult::P_UnknownError;

	result = sendAllPacket(soc, data.buffer.data(), (int)data.buffer.size());

	if(result != PResult::P_Success)
		return PResult::P_UnknownError;

	return PResult::P_Success;
}

PResult Socket::recvPacket(const SocketData& soc, void* dest, int numberOfBytes, int& bytesRecv)
{
	if(!soc.hnd)
	{
		LastNetworkError::SetLastError("send packet error: hnd not initalized");
		return PResult::P_UnknownError;
	}

	bytesRecv = recv(soc.hnd, (char*)dest, numberOfBytes, NULL);

	if(!bytesRecv)// if connection was gracefully closed
		return PResult::P_UnknownError;


	if(bytesRecv == SOCKET_ERROR)
	{


		LastNetworkError::SetLastError("recv packet error: ", WSAGetLastError());

		return PResult::P_UnknownError;
	}

	return PResult::P_Success;
}

PResult Socket::recvPacket(const SocketData& soc, Packet& dest)
{
	dest.clear();

	uint16_t encodedSize = 0;
	PResult result = recvAllPacket(soc, &encodedSize, sizeof(uint16_t));
	if(result != PResult::P_Success)
		return PResult::P_UnknownError;

	encodedSize = (uint16_t)ntohl((u_long)encodedSize);

	if(encodedSize > MAX_PACKET_SIZE)
		return PResult::P_UnknownError;

	dest.buffer.resize(encodedSize);
	result = recvAllPacket(soc, dest.buffer.data(), encodedSize);

	if(result != PResult::P_Success)
		return PResult::P_UnknownError;


	return PResult::P_Success;
}

PResult Socket::sendAllPacket(const SocketData& soc, void* data, int numberOfBytes)
{
	int sentBytes = 0, totalBytes = 0, bitesRemaining = 0;
	//PResult result = PResult::P_Success;
	while(numberOfBytes > totalBytes)
	{
		bitesRemaining = numberOfBytes - totalBytes;
		if((sendPacket(soc, (char*)data + totalBytes, bitesRemaining, sentBytes))
		   != PResult::P_Success)
			return PResult::P_UnknownError;
		totalBytes += sentBytes;
	}

	return PResult::P_Success;
}

PResult Socket::recvAllPacket(const SocketData& soc, void* dest, int numberOfBytes)
{
	int recvBytes = 0, totalBytes = 0, bitesRemaining = 0;
	//PResult result = PResult::P_Success;

	while(numberOfBytes > totalBytes)
	{
		bitesRemaining = numberOfBytes - totalBytes;
		if(recvPacket(soc, (char*)dest + totalBytes, bitesRemaining, recvBytes)
		   != PResult::P_Success)
			return PResult::P_UnknownError;

		totalBytes += recvBytes;
	}
	return PResult::P_Success;
}

PResult Socket::recvFromPacket(const SocketData& soc, void* dest, int numberOfBytes, int& bytesRecv, IPEndpointData& ep)
{
	if(soc.IPv != ep.IPv)
	{
		LastNetworkError::SetLastError("receve from packet error: IP versions do not match");
		return PResult::P_UnknownError;
	}
	if(!soc.hnd)
	{
		LastNetworkError::SetLastError("recv from packet error: hnd not initalized");
		return PResult::P_UnknownError;
	}
	sockaddr_in client;

	static int clientLength = sizeof(sockaddr_in);
	bytesRecv = recvfrom(soc.hnd, (char*)dest, numberOfBytes, NULL, (sockaddr*)&client, &clientLength);

	if(!bytesRecv)// if connection was gracefully closed
		return PResult::P_UnknownError;


	if(bytesRecv == SOCKET_ERROR)
	{
		LastNetworkError::SetLastError("receve from packet error: ", WSAGetLastError());

		return PResult::P_UnknownError;
	}


	ep = IPEndpoint::createIPEndpoint((sockaddr*)&client);//return handle to client

	return PResult::P_Success;
}

PResult Socket::recvFromPacket(const SocketData& soc, void* data, int numberOfBytes, int& bytesSent)
{
	static IPEndpointData tmp;
	return recvFromPacket(soc, data, numberOfBytes, bytesSent, tmp);
}

PResult Socket::recvFromPacket(const SocketData& soc, void* data, int numberOfBytes, IPEndpointData& ep)
{
	static int dump = 0;
	return recvFromPacket(soc, data, numberOfBytes, dump, ep);
}

PResult Socket::recvFromPacket(const SocketData& soc, void* data, int numberOfBytes)
{
	static IPEndpointData tmp;
	return recvFromPacket(soc, data, numberOfBytes, tmp);
}

PResult Socket::sendToPacket(const SocketData& soc, const void* data, int numberOfBytes, int& bytesSent, const IPEndpointData& ep)
{
	if(soc.IPv != ep.IPv)
	{
		LastNetworkError::SetLastError("send to packet error: IP versions do not match");
		return PResult::P_UnknownError;
	}
	if(!soc.hnd)
	{
		LastNetworkError::SetLastError("send to packet error: hnd not initalized");
		return PResult::P_UnknownError;
	}
	sockaddr_in server = IPEndpoint::getSockAddrIPv4(ep);

	static int serverLength = sizeof(server);

	bytesSent = sendto(soc.hnd, (const char*)data, numberOfBytes, NULL, (sockaddr*)&server, serverLength);

	if(bytesSent == SOCKET_ERROR)
	{
		LastNetworkError::SetLastError("send to packet error: ", WSAGetLastError());

		return PResult::P_UnknownError;
	}
	return PResult::P_Success;
}

PResult Socket::sendToPacket(const SocketData& soc, const void* data, int numberOfBytes, const IPEndpointData& ep)
{
	static int dump;
	return sendToPacket(soc, data, numberOfBytes, dump, ep);
}


PResult Socket::initTCP(SocketData& data, bool block)
{
	if(!(data.IPv == IPVersion::IPv4 || data.IPv == IPVersion::IPv6))
	{
		LastNetworkError::SetLastError("init TCP error: unknown IP version");
		return P_UnknownError;
	}
	if(data.hnd != INVALID_SOCKET)
	{
		LastNetworkError::SetLastError("init TCP error: SocketData not initialized");
		return P_UnknownError;
	}

	if(data.IPv == IPVersion::IPv6)
		data.hnd = socket(AF_INET6, SOCK_STREAM, IPPROTO_TCP);
	else//IPv4
		data.hnd = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

	if(data.hnd == INVALID_SOCKET)
	{
		LastNetworkError::SetLastError("init TCP error: ", WSAGetLastError());
		return P_UnknownError;
	}

	BOOL tru = TRUE;
	if(setSocketOption(data, TCP_NoDelay, &tru))
		return P_UnknownError;

	if(setBlocking(data, block) != PResult::P_Success)
		PResult::P_UnknownError;

	return PResult::P_Success;
}

PResult Socket::initUDP(SocketData& data, bool block)
{
	if(!(data.IPv == IPVersion::IPv4 || data.IPv == IPVersion::IPv6))
	{
		LastNetworkError::SetLastError("init UDP error: unknown IP version");
		return P_UnknownError;
	}
	if(data.hnd != INVALID_SOCKET)
	{
		LastNetworkError::SetLastError("init UDP error: SocketData not initialized");
		return P_UnknownError;
	}

	if(data.IPv == IPVersion::IPv6)
		data.hnd = socket(AF_INET6, SOCK_DGRAM, IPPROTO_IP/*0*/);
	else//IPv4
		data.hnd = socket(AF_INET, SOCK_DGRAM, IPPROTO_IP/*0*/);

	if(data.hnd == INVALID_SOCKET)
	{
		LastNetworkError::SetLastError("init UDP error: ", WSAGetLastError());
		return P_UnknownError;
	}

	if(setBlocking(data, block) != PResult::P_Success)
		PResult::P_UnknownError;

	//if(setSocketOption(TCP_NoDelay, TRUE))
	//	return P_GenericError;

	return P_Success;
}

PResult Socket::setSocketOption(const SocketData& soc, SocketOption opt, void* val)
{
	if(!soc.hnd)
	{
		LastNetworkError::SetLastError("set socket options error: hnd not initalized");
		return PResult::P_UnknownError;
	}
	int result = 0;
	switch(opt)
	{
	case SocketOption::TCP_NoDelay:
		result = setsockopt(soc.hnd, IPPROTO_TCP, TCP_NODELAY, (const char*)val, sizeof(BOOL));
		break;
	case SocketOption::IPV6_Only:
		result = setsockopt(soc.hnd, IPPROTO_IPV6, IPV6_V6ONLY, (const char*)val, sizeof(BOOL));
		break;

	default:
		//	setsockopt(m_hnd, SOL_SOCKET, SO_RCVTIMEO, (const char*)val, sizeof(unsigned));
		return P_UnknownError;
	}

	if(result)
	{


		LastNetworkError::SetLastError("socket options error: ", WSAGetLastError());

		return P_UnknownError;
	}


	return P_Success;
}

