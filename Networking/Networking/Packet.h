#pragma once
#define WIN32_LEAN_AND_MEAN

#include <WinSock2.h>
#include <vector>
#include "PacketError.h"

#define MAX_PACKET_SIZE 8154

enum PacketType: uint16_t
{
	PT_Invalid,
	PT_CheatMessage,
	PT_IntegerArray
};

struct Packet
{
	Packet(PacketType type = PT_Invalid);
	PacketType getPacketType();
	void assignPacketType(PacketType type);

	void clear();
	void append(const void* data, uint32_t size);

	Packet& operator<<(uint32_t data);
	Packet& operator>>(uint32_t& data);

	Packet& operator<<(const std::string data);
	Packet& operator>>(std::string& data);

	template<class T>
	Packet& operator<<(T data)
	{
		this-> operator<<( (uint32_t)sizeof(data));
		append(&data);
		extractOffset += sizeof(data);
	}

	template<class T>
	Packet& operator>>(T data)
	{

	}



	uint32_t extractOffset = 0;
	std::vector<char> buffer;
};


