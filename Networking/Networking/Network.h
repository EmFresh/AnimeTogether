#pragma once
#define WIN32_LEAN_AND_MEAN
#include<WinSock2.h>

class Network
{
public :
	static bool init();
	static bool shutdown();
};

