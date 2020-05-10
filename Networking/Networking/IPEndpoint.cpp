#include "IPEndpoint.h"

IPEndpointData IPEndpoint::createIPEndpoint(const char* ip, unsigned short port, IPVersion ipv, int flags)
{
	IPEndpointData data;

	data.port = port;

	in_addr addr; in6_addr addr6;

	int result;
	if(ipv == IPVersion::IPv6)
		result = inet_pton(AF_INET6, ip, &addr6);
	else
		result = inet_pton(AF_INET, ip, &addr);

	if(result == 1)
	{
		//
		unsigned length = (unsigned)strlen(ip) + 1;
		if(data.ipString)
			delete[] data.ipString;

		data.ipString = new char[length];
		memcpy_s((char*)data.ipString, length, ip, length);

		if(data.hostname)
			delete[] data.hostname;
		data.hostname = new char[length];
		memcpy_s((char*)data.hostname, length, ip, length);

		if(data.ipBytes)
			delete[] data.ipBytes;

		if(ipv == IPVersion::IPv6)
			data.ipBytesSize = 16;
		else
			data.ipBytesSize = sizeof(ULONG);

		data.ipBytes = new uint8_t[data.ipBytesSize];
		if(ipv == IPVersion::IPv6)
			memcpy_s(data.ipBytes, data.ipBytesSize, &addr6.u, data.ipBytesSize);
		else
			memcpy_s(data.ipBytes, data.ipBytesSize, &addr.S_un.S_addr, data.ipBytesSize);

		data.IPv = ipv;
		return data;
	}

	addrinfo hints{};
	if(ipv == IPVersion::IPv6)
		hints.ai_family = AF_INET6;
	else
		hints.ai_family = AF_INET;

	hints.ai_flags = flags;
	addrinfo* hostinfo = nullptr;
	result = getaddrinfo(std::string(ip) == "" ? NULL : ip, nullptr, &hints, &hostinfo);

	if(!result)//attempt to create IP from host name (i.e. localhost) 
	{
		sockaddr_in* hostAddr = (sockaddr_in*)hostinfo->ai_addr;
		sockaddr_in6* hostAddr6 = (sockaddr_in6*)hostAddr;
		//hostAddr->sin_addr.S_un.S_addr;

		if(data.ipString)
			delete[] data.ipString;

		int dataSize;
		if(ipv == IPVersion::IPv6)
			dataSize = 46;
		else
			dataSize = 16;

		data.ipString = new char[dataSize];
		if(ipv == IPVersion::IPv6)
			inet_ntop(AF_INET6, &hostAddr6->sin6_addr, (char*)data.ipString, dataSize);
		else
			inet_ntop(AF_INET, &hostAddr->sin_addr, (char*)data.ipString, dataSize);

		if(data.hostname)
			delete[] data.hostname;
		unsigned length = (unsigned)strlen(ip) + 1;
		data.hostname = new char[length];
		memcpy_s((char*)data.hostname, length, ip, length);

		ULONG ipLong = hostAddr->sin_addr.S_un.S_addr;

		if(ipv == IPVersion::IPv6)
			dataSize = 16;
		else
			dataSize = sizeof(ULONG);//v4

		if(data.ipBytes)
			delete[] data.ipBytes;
		data.ipBytes = new uint8_t[dataSize];
		data.ipBytesSize = dataSize;
		if(ipv == IPVersion::IPv6)
			memcpy_s(data.ipBytes, dataSize, &hostAddr6->sin6_addr, dataSize);
		else
			memcpy_s(data.ipBytes, dataSize, &ipLong, dataSize);

		data.IPv = ipv;

		freeaddrinfo(hostinfo);
		return data;
	}

	data.IPv = ipv;//prevents errors in dll
	return data;
}

IPEndpointData IPEndpoint::createIPEndpoint(const sockaddr* addr, IPVersion ipv)
{
	assert(addr->sa_family == AF_INET || addr->sa_family == AF_INET6);

	IPEndpointData data;

	sockaddr_in* addrv4 = reinterpret_cast<sockaddr_in*>((sockaddr*)addr);
	sockaddr_in6* addrv6 = reinterpret_cast<sockaddr_in6*>(addrv4);


	data.IPv = ipv;
	if(ipv == IPVersion::IPv6)
		data.port = ntohs(addrv6->sin6_port);
	else//IPv4
		data.port = ntohs(addrv4->sin_port);

	int dataSize;
	if(ipv == IPVersion::IPv6)
		dataSize = 16;
	else//IPv4
		dataSize = sizeof(ULONG);

	if(data.ipBytes)
		delete[] data.ipBytes;
	data.ipBytes = new uint8_t[dataSize];
	data.ipBytesSize = dataSize;
	if(ipv == IPVersion::IPv6)
		memcpy_s(&data.ipBytes[0], dataSize, &addrv6->sin6_addr, dataSize);
	else
		memcpy_s(&data.ipBytes[0], dataSize, &addrv4->sin_addr, dataSize);


	if(ipv == IPVersion::IPv6)
		dataSize = 46;
	else//IPv4
		dataSize = 16;

	if(data.ipString)
		delete[] data.ipString;
	data.ipString = new char[dataSize];

	if(ipv == IPVersion::IPv6)
		inet_ntop(AF_INET6, &addrv6->sin6_addr, (char*)data.ipString, dataSize);
	else
		inet_ntop(AF_INET, &addrv4->sin_addr, (char*)data.ipString, dataSize);

	data.hostname = data.ipString;
	return data;
}

sockaddr_in IPEndpoint::getSockAddrIPv4(const IPEndpointData& data)
{
	assert(data.IPv == IPVersion::IPv4);
	sockaddr_in addr = {};
	addr.sin_family = AF_INET;

	//addr.sin_addr.S_un.S_addr = ADDR_ANY;//not sure if this is specific for UDP?NVM it's just zero???
	if(data.ipBytes)
		memcpy_s(&addr.sin_addr, sizeof(ULONG), data.ipBytes, sizeof(ULONG));

	addr.sin_port = htons(data.port);
	return addr;
}

sockaddr_in6 IPEndpoint::getSockAddrIPv6(const IPEndpointData& data)
{
	assert(data.IPv == IPVersion::IPv6);
	sockaddr_in6 addr = {};
	addr.sin6_family = AF_INET6;

	//addr.sin_addr.S_un.S_addr = ADDR_ANY;//not sure if this is specific for UDP?NVM it's just zero???
	if(data.ipBytes)
		memcpy_s(&addr.sin6_addr, 16, data.ipBytes, 16);

	addr.sin6_port = htons(data.port);
	return addr;
}
