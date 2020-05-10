#include "Socket.h"
#include "Network.h"
#include "IPEndpoint.h"


//int main()
//{
//	if(Network::init())
//	{
//		IPEndpoint endp("192.168.0.2", 8080);
//	
//		if(endp.getIPversion() == IPv4)
//		{
//			endp.print();
//		}
//	
//		Socket sock;
//		if(sock.create() == P_Success)
//			while(true)
//			{
//	
//			}
//	}
//	
//	Network::shutdown();
//}