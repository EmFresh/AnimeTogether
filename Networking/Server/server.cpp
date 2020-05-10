#include <Networking/IncludeThis.h>
#include <iostream>
#include <thread>
#include "keyinput.h"
using std::cin;

struct vec3
{
	float x = 0, y = 0, z = 0;

	void print()
	{
		printf("(%.2f, %.2f, %.2f)", x, y, z);
	}
};

//#define UDPIMP
#define TCPIMP
//#define LILIANSSERVER

#ifdef UDPIMP

int main()
{
	if(Network::init())
	{
		IPEndpointData endp = IPEndpoint::createIPEndpoint("localhost", 21);

		if(endp.IPv == IPVersion::IPv4)
		{
			endp.print();
			puts("");

			SocketData sock = Socket::createSocketData();

			if(Socket::init(sock, UDP) == PResult::P_Success)
			{
				printf("Created Socket\n");
				if(Socket::bindEndpoint(endp, sock) == PResult::P_Success)
				{
					puts("Bind Endpoint");

					auto result = P_Success;
					while(true)
					{
						static int size, dump;

						vec3 dat;
						if(Socket::recvFromPacket(sock, &size, 4, dump, endp) == P_Success)
						{
							//printf("%s", LastNetworkError::GetLastError().c_str());
							if(size > 0)
								if(Socket::recvFromPacket(sock, &dat, size, dump, endp) == P_Success)
								{
									if(dump == sizeof(vec3))
										dat.print();
									puts("\n");
								}
						}

						//printf("Size: %d\n", size);
					}

				}
				Socket::close(sock);
			}
		}
	}

	Network::shutdown();
	system("pause");
}
#endif // UDPIMP

#ifdef TCPIMP

void  sendMesages(std::vector<SocketData>* sockets)
{
	std::string msg;

	while(true)
	{
		std::getline(std::cin, msg);
		int size = ((int)msg.size()) + 1;

		for(auto& sock : *sockets)
		{
			//if(Socket::pollEvents(sock, 1, EventsPoll::EP_OUT) != PResult::P_Success)
			//{
			//	puts(LastNetworkError::GetLastError().c_str());
			//	continue;
			//}
			Socket::sendAllPacket(sock, &size, 4);
			Socket::sendAllPacket(sock, (char*)msg.data(), size);
		}
	}
}

void recvMesages(std::vector<SocketData>* sockets)
{

	std::string msg;
	int size;
	for(auto& sock : *sockets)
	{
		size = 0;
		//if(Socket::pollEvents(sock, 1, EventsPoll::EP_IN) != PResult::P_Success)
		//{
		//	puts(LastNetworkError::GetLastError().c_str());
		//	continue;
		//}
		if(Socket::recvAllPacket(sock, &size, 4) == P_UnknownError)
			puts(LastNetworkError::GetLastError().data());
		msg.resize(size);
		if(Socket::recvAllPacket(sock, (char*)msg.data(), size) == P_UnknownError)
			puts(LastNetworkError::GetLastError().data());
		printf("Message Received: %s\n", msg.data());
	}

}

std::vector<SocketData> newSock;
void frame()
{
	//	puts("Done");
	//	auto result = P_Success;
	//	std::thread snd(sendMesages, newSock)/*, rec(recvMesages, newSock)*/;
	//	recvMesages(newSock);
	//	//while(true);
}

int main()
{
	std::string ip;
	int port;
	printf("enter IP: ");
	cin >> ip;
	printf("enter port#: ");
	cin >> port;
	std::thread snd(sendMesages, &newSock);
	std::thread msg(recvMesages, &newSock);
	if(Network::init())
	{
		IPEndpointData endp = IPEndpoint::createIPEndpoint(ip.data(), port, IPVersion::IPv4);
		SocketData sock = Socket::createSocketData(IPVersion::IPv4);
		//newSock.push_back(sock);
		//192.168.0.12
		endp.print();
		puts("");


		printf("Create Socket: ");
		if(Socket::init(sock, TCP, true) == PResult::P_Success)
		{
			puts("Done");

			printf("Listen Endpoint: ");
			if(Socket::listenEndpoint(endp, sock) == PResult::P_Success)
			{
				puts("Done");
				//if(Socket::pollEvents(sock, 1, EventsPoll::EP_RDNORM) == PResult::P_UnknownError)
				//{
				//	puts(LastNetworkError::GetLastError().data());
				//	continue;
				//}

				//if(sock.revents & REventsPoll::REP_RDNORM)
				{
					while(!GetAsyncKeyState(VK_ESCAPE))
					{

						printf("Accept Socket: ");
						newSock.push_back(Socket::createSocketData(IPVersion::IPv4));
						if(Socket::acceptSocket(sock, newSock.back()) != PResult::P_Success)
							puts("Error");
						else
							puts("Done");


						puts(LastNetworkError::GetLastError().data());
					}

				}

			}
			else
				puts("Error"),
				puts(LastNetworkError::GetLastError().data());


			Socket::close(sock);
			for(auto& closing : newSock)
				Socket::close(closing),
				Socket::close(closing);

		}
		printf("%s\n", LastNetworkError::GetLastError().c_str());

	}

	Network::shutdown();
	system("pause");
}

#endif // TCPIMP

#ifdef LILIANSSERVER
//takes a string and splits it using the seperator character(s)
std::vector<std::string> split(std::string str, const char* seperators)
{
	std::vector<std::string> ans;
	char* tmp = strtok_s((char*)str.c_str(), seperators, nullptr);
	ans.push_back(tmp);

	while(tmp)
	{
		tmp = strtok_s(nullptr, seperators, nullptr);
		if(tmp)	ans.push_back(tmp);
	}
	return ans;
}

struct User
{
	std::string _name;
	IPEndpointData _adress;
	int _index;
	User(std::string name, IPEndpointData adress, int index)
	{
		_name = name;
		_adress = adress;
		_index = index;
	}
};

class AllUsers
{

public:
	std::list <User> listUsers;

	//Add one user to list
	void addUser(std::string name, IPEndpointData adress, int index)
	{
		listUsers.push_back(User(name, adress, index));
	}
	//Remove user in list of certain ID
	void removeUser(int id)
	{
		for(std::list <User> ::iterator it = listUsers.begin(); it != listUsers.end(); it++)
		{
			if((*it)._index == id)
			{
				listUsers.erase(it);
				return;
			}
		}
		printf("Id of user is not exit!!!!!Cannot remove user!!!\n");
	}
	//Get name of a user in certain ID
	std::string getName(int id)
	{
		for(std::list <User> ::iterator it = listUsers.begin(); it != listUsers.end(); it++)
		{
			if((*it)._index == id)
			{
				return (*it)._name;
			}
		}
		printf("Id of user is not exit!!!!!Name cannot be found!!!\n");
		return std::string();
	}
	//Get adress of user in certain ID
	IPEndpointData getAdress(int id)
	{
		for(std::list <User> ::iterator it = listUsers.begin(); it != listUsers.end(); it++)
		{
			if((*it)._index == id)
			{
				return (*it)._adress;
			}
		}
		printf("Id of user is not exit!!!!!Adress cannot be found!!!\n");
		return IPEndpointData();
	}
	int getNewID()
	{
		if(listUsers.size() == 0)
		{
			return 1;
		}
		else
		{
			return listUsers.back()._index++;
		}
	}

};

#define PORT 8888
#define IP   "0.0.0.0"
#define BUF_LEN 512
#define reclass(a_class, a_val) (*(a_class*)&(a_val))

enum class ServerStatus:int
{
	Lobby = 1,
	Game = 2,
};

enum class MessageType:int
{
	Unknown,
	Movement,
};

class Server
{
public:
	Server() { CreateServer(); }
	~Server() {}

	void CreateServer();// Initialize the server
	void UpdateRecv();// Handle the receiving loop
	void UpdateSend();// Handle the sending loop
	void HandleSending(const void* _message, IPEndpointData _adress);// Send a string to certain adress
	void BroadcastMessageToAll(const void* _message);// Send a string to all users (Also print in command)
	void CloseServer();// Shutdown server

	void join(std::string _name, IPEndpointData _adress);// Process when a new player join server
	bool checkAllStartPressed();// Check is all seat occupied or not
	void takeSeat(int _userId, int _seatId);// Process when user try to take a seat
	void leftSeat(int _id);// Process when user left the seat
	void leftLobby(int _id);// Process when user left the lobby
	void pressedStart(int _userId);// Process when user press the start
	void leftGame(int _id);// Process when user left during the gameplay

	bool isServerRunning = false;

private:
	char recv_buf[BUF_LEN];

	AllUsers users;
	int lobbySeat[4];
	int startButtons[4];
	ServerStatus status = ServerStatus::Lobby;
	SocketData socket;
	IPEndpointData ip;
};

void Server::CreateServer()
{
	if(!Network::init())return;

	ip = IPEndpoint::createIPEndpoint(IP, PORT, AI_PASSIVE);
	socket = Socket::createSocketData();

	Socket::init(socket, UDP);
	Socket::bindEndpoint(ip, socket);

	printf("Server is now running!\n");
	isServerRunning = true;
}

void Server::UpdateRecv()
{

	static IPEndpointData fromAddr;

	while(isServerRunning)
	{
		if(status == ServerStatus::Lobby)
		{

			memset(recv_buf, 0, BUF_LEN);
			if(Socket::recvFromPacket(socket, recv_buf, BUF_LEN, fromAddr) == P_UnknownError)
			{
				puts(LastNetworkError::GetLastError().c_str());
				continue;//goes to the top of the while loop
			}

			// TODO:
			std::string message = recv_buf;
			char code = message[0];

			//alternitive method (looks a bit cleaner... thats all)//
			int userId, seatId;
			switch(code)
			{
			case '@':// Join require
				message.erase(0, 1);
				join(message, fromAddr);
				break;

			case '#':// Seat require
				message.erase(0, 1);
				seatId = message[0] - '0';
				message.erase(0, 1);
				userId = std::stoi(message);
				takeSeat(userId, seatId);
				break;

			case '$':// Player leave seat
				message.erase(0, 1);
				userId = std::stoi(message);
				leftSeat(userId);
				break;

			case '%':// Player pressed start
				message.erase(0, 1);
				userId = std::stoi(message);
				pressedStart(userId);
				break;

			case '&':// Player leave Lobby
				message.erase(0, 1);
				userId = std::stoi(message);
				leftGame(userId);
				break;

			default:
				printf("Received: %s\n", recv_buf);
			}

		}
		else if(status == ServerStatus::Game)
		{
			int fromlen = 0;

			if(Socket::recvFromPacket(socket, recv_buf, BUF_LEN, fromlen, fromAddr) == P_UnknownError)
			{
				printf("%s\n", LastNetworkError::GetLastError().c_str());
				continue;
			}

			if(fromlen > 8)
			{
				MessageType msg = (MessageType)reclass(int, recv_buf);
				switch(msg)
				{
					//TODO: create movement structure here
				case MessageType::Movement:
					if(fromlen >= reclass(int, recv_buf[4]))//check for the correct data size
					{
						puts("recived Movement message from client");
						//TODO: get player ID,Position,velocity,elapsed time and broadcast
						BroadcastMessageToAll(recv_buf);//send it to everyone
					}

					break;
				default:

					break;
				}
			}

			// TODO:
			// Player leave game
		}
		else
		{
			printf("Player status is incorrect!!!!");
		}
	}
}

void Server::UpdateSend()
{
	while(isServerRunning)
	{
		if(status == ServerStatus::Lobby)
		{
			//std::string line;
			//std::getline(std::cin, line);

			//if (line.size() > 0)
			//	//BroadcastMessageToAll(line);
			//	//////For Testing
			//	HandleSending(line, temp);

			// check for start game
			if(checkAllStartPressed())
			{
				status = ServerStatus::Game;
				//TODO:
				BroadcastMessageToAll("$");
			}
		}
		else if(status == ServerStatus::Game)
		{

		}
		else
		{
			puts("Player status is incorrect!!!!");
		}
	}
}

void Server::HandleSending(const void* _message, IPEndpointData _adress)
{
	if(Socket::sendToPacket(socket, _message, BUF_LEN, _adress) == P_UnknownError)
	{
		printf("sendto() failed %d\n", WSAGetLastError());
	}
}

void Server::BroadcastMessageToAll(const void* _message)
{
	std::cout << "Send All: " << std::string((char*)_message) << std::endl;
	if(users.listUsers.size() != 0)
	{
		for(std::list <User> ::iterator it = users.listUsers.begin(); it != users.listUsers.end(); it++)
		{
			HandleSending(_message, (*it)._adress);
		}
	}
}

void Server::CloseServer()
{
	puts("Server shutdown");
	Socket::close(socket);
	Network::shutdown();
	isServerRunning = false;
}

void Server::join(std::string _name, IPEndpointData _adress)
{
	int userId = users.listUsers.size() + 1;
	std::cout << "UserID: " << std::to_string(userId) << std::endl;

	//// Notice all player in lobby
	std::string newMassage = "!" + _name + ":" + std::to_string(userId);
	BroadcastMessageToAll(newMassage.c_str());

	//// Sendback conformation, all other online player info, id, start info and seat info
	std::string sendBack;
	sendBack += "@";
	sendBack += std::to_string(userId);
	sendBack += ":";
	// number of user in server
	sendBack += std::to_string(users.listUsers.size());
	// users info
	if(users.listUsers.size() != 0)
	{
		for(std::list <User> ::iterator it = users.listUsers.begin(); it != users.listUsers.end(); it++)
		{
			sendBack += ":";
			sendBack += (*it)._name;
			sendBack += ":";
			sendBack += std::to_string((*it)._index);
		}
	}
	// seat info 
	for(int i = 0; i < 4; i++)
	{
		sendBack += ":";
		sendBack += std::to_string(lobbySeat[i]);
	}
	// start Info
	for(int i = 0; i < 4; i++)
	{
		sendBack += ":";
		sendBack += std::to_string(startButtons[i]);
	}
	// send
	HandleSending(sendBack.c_str(), _adress);

	//// Add to player list
	users.addUser(_name, _adress, userId);
}

bool Server::checkAllStartPressed()
{
	bool check = true;
	for(int i = 0; i < 4; i++)
	{
		if(startButtons[i] == 0)
		{
			check = false;
			break;
		}
	}
	return check;
}

void Server::takeSeat(int _userId, int _seatId)
{
	if(lobbySeat[_seatId] == 0)
	{
		lobbySeat[_seatId] = _userId;
		// Update seat changing to all user
		std::string seatInfo = "#";
		for(int i = 0; i < 4; i++)
		{
			seatInfo += ":";
			seatInfo += std::to_string(lobbySeat[i]);
		}
		BroadcastMessageToAll(seatInfo.c_str());
	}
	else
	{
		// Seat is occupied
		HandleSending("Seat is Occupied, please choose another one.", users.getAdress(_userId));
	}
}

void Server::leftSeat(int _id)
{
	// Remove user from seat
	for(int i = 0; i < 4; i++)
	{
		if(lobbySeat[i] == _id)
		{
			lobbySeat[i] = 0;
			break;
		}
	}
	// Update seat changing to all user
	std::string seatInfo = "#";
	for(int i = 0; i < 4; i++)
	{
		seatInfo += ":";
		seatInfo += std::to_string(lobbySeat[i]);
	}
	BroadcastMessageToAll(seatInfo.c_str());
}

void Server::leftLobby(int _id)
{
	// Remove user from list of user
	users.removeUser(_id);

	// Let all user know
	//TODO:
	BroadcastMessageToAll("");
}

void Server::pressedStart(int _userId)
{
	int _id = 0;
	for(int i = 0; i < 4; i++)
	{
		if(lobbySeat[i] == _userId)
		{
			_id = i;
			break;
		}
	}
	startButtons[_id] = _userId;

	// Update seat changing to all user
	std::string startInfo = "%";
	for(int i = 0; i < 4; i++)
	{
		startInfo += ":";
		startInfo += std::to_string(startButtons[i]);
	}
	BroadcastMessageToAll(startInfo.c_str());
}

void Server::leftGame(int _id)
{
	// Remove user from list of user
	users.removeUser(_id);

	// Let all user know
	//TODO:
	BroadcastMessageToAll("");
}

int main()
{
	static Server myServer;
	atexit([](){myServer.CloseServer(); });//cleans up server once program exits
	//myServer.CreateServer();

	std::thread receiveThread = std::thread([](){myServer.UpdateRecv();	});
	std::thread sendThread = std::thread([](){myServer.UpdateSend(); });

	while(myServer.isServerRunning);
	return 0;
}
#endif // LILIANSSERVER
