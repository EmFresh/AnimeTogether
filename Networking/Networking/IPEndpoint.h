#pragma once
#include <string>
#include <vector>
#include <cassert>
#include <WS2tcpip.h>
#include "IPVersion.h"
typedef const char* cstring;

struct IPEndpointData
{
	IPVersion IPv = IPVersion::IPUnknown;
	cstring hostname = nullptr;
	cstring ipString = nullptr;
	ULONG ipBytesSize = 0;
	uint8_t* ipBytes = nullptr;
	unsigned short port = 0;

	void print()
	{
		switch(IPv)
		{
		case IPVersion::IPv4:
			printf("IP Version: IPv4\n");
			break;
		case IPVersion::IPv6:
			printf("IP Version: IPv6\n");
			break;
		default:
			printf("IP Version: Unknown\n");
		}

		printf
		(
			"HostName: %s\n"
			"Port: %d\n"
			"IP: %s\n"
			"IP bytes... ",
			hostname, port, ipString
		);

		for(int a = 0; a < (int)ipBytesSize; ++a)
			printf("%d.", int(ipBytes[a]));
		puts("\b \b");
	}
};

class IPEndpoint
{
public:
	static IPEndpointData createIPEndpoint(const char* ip, unsigned short port, IPVersion ipv = IPVersion::IPv4, int ai_flags = 0);


	//NETWORK USE ONLY//

	static IPEndpointData createIPEndpoint(const sockaddr* addr, IPVersion ipv = IPVersion::IPv4);
	static sockaddr_in getSockAddrIPv4(const IPEndpointData&);
	static sockaddr_in6 getSockAddrIPv6(const IPEndpointData&);
};

