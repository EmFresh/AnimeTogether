#include <Networking/IncludeThis.h>
#include <ctime>
#include <iostream>
#include <thread>
#include "keyinput.h"
using std::cin;

struct vec3
{
	float x = 0, y = 0, z = 0;

	vec3 operator+(vec3 in)
	{
		return {x + in.x, y + in.y, z + in.z};
	}
	vec3 operator-(vec3 in)
	{
		return {x - in.x, y - in.y, z - in.z};
	}
	bool operator==(vec3 in)
	{
		return x == in.x && y == in.y && z == in.z;
	}

	bool operator!=(vec3 in)
	{
		return !(*this == in);
	}

	float length()
	{
		return sqrt(x * x + y * y + z * z);
	}
	static float dist(vec3 v1, vec3 v2)
	{
		return (v1 - v2).length();
	}

	void print()
	{
		printf("(%f,%f,%f)\n", x, y, z);
	}
};

//#define UDPIMP
#define TCPIMP
#ifdef UDPIMP

int main()
{
	if(Network::init())
	{
		IPEndpointData endp = IPEndpoint::createIPEndpoint("localhost", 21);


		if(endp.IPv == IPVersion::IPv4)
		{
			endp.print();
		}

		SocketData sock = Socket::createSocketData();

		if(Socket::init(sock, UDP) == PResult::P_Success)
		{
			printf("Created Socket\n");
			if(Socket::connectEndpoint(endp, sock) == PResult::P_Success)
			{
				printf("conncted to endpoint\n");

				while(true)
				{


					static vec3 dat, last;
					if(KeyInput::press('a'))dat = dat - vec3{.001f,0,0};
					if(KeyInput::press('w'))dat = dat + vec3{0,.001f,0};
					if(KeyInput::press('s'))dat = dat - vec3{0,.001f,0};
					if(KeyInput::press('d'))dat = dat + vec3{.001f,0,0};

					int size = sizeof(dat);
					bool sending = true;
					if(vec3::dist(last, dat) >= .5f)
					{
						if(Socket::sendToPacket(sock, &size, 4, endp) == P_UnknownError)
							sending = false;
						if(Socket::sendToPacket(sock, &dat, size, endp) == P_UnknownError)
							sending = false;

						if(sending)
							dat.print();

						last = dat;
					}

				}//delete[] str;
			}
			Socket::close(sock);
		}
	}

	Network::shutdown();
	system("pause");
}
#endif // UDPIMP

#ifdef TCPIMP

SocketData sock = Socket::createSocketData(IPVersion::IPv4);
void  sendMesages()
{

	std::string msg;
	while(true)
	{
		if(sock.hnd == INVALID_SOCKET)continue;

		std::getline(std::cin, msg);
		int size = ((int)msg.size()) + 1, dump = 0;
		if(Socket::sendAllPacket(sock, &size, 4) == P_UnknownError)
			puts(LastNetworkError::GetLastError().data());
		if(Socket::sendAllPacket(sock, (char*)msg.data(), size) == P_UnknownError)
			puts(LastNetworkError::GetLastError().data());


	}
}

void recvMesages()
{

	int size = 0, dump = 0;
	std::string msg;
	while(true)
	{
		if(sock.hnd == INVALID_SOCKET)continue;

		Socket::recvPacket(sock, &size, 4, dump);
		msg.resize(size);
		Socket::recvPacket(sock, (char*)msg.data(), size, dump);
		printf("Message Recieved: %s\n", msg.data());
	}
}

IPEndpointData endp;
int main()
{
	std::string ip;
	int port;
	printf("enter IP: ");
	cin >> ip;
	printf("enter port#: ");
	cin >> port;
	endp = IPEndpoint::createIPEndpoint(ip.data(), port, IPVersion::IPv4);
	if(Network::init())
	{

		endp.print();
		puts("");


		if(Socket::init(sock, TCP) == PResult::P_Success)
		{
			printf("Created Socket\n");
			while(true)
			{
				if(Socket::connectEndpoint(endp, sock) == PResult::P_Success)
				{
					std::thread snd(sendMesages), rec(recvMesages);
					puts("Connect Endpoint");
					auto result = P_Success;
					while(true);
				}
				printf("%s\n", LastNetworkError::GetLastError().c_str());
				Socket::close(sock);
			}
		}


		Network::shutdown();
		system("pause");
	}
}
#endif // TCPIMP
