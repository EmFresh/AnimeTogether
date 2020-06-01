#include "Network.h"
#include "LastNetworkError.h"
#include <cstdio>

bool Network::init()
{
	WSADATA wsaData;
	int resault = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if(resault)
	{
		LastNetworkError::SetLastError("network init error: ", WSAGetLastError());
		return false;
	}

	if(LOBYTE(wsaData.wVersion)!=2 || HIBYTE(wsaData.wVersion) != 2)
	{
		LastNetworkError::SetLastError("network init version error: ", WSAGetLastError());
		return false;
	}

	return true;
}

bool Network::shutdown()
{
	if(WSACleanup() == SOCKET_ERROR)
	{
		LastNetworkError::SetLastError("network shutdown error: ", WSAGetLastError());
		return false;
	}
	return true;
}
