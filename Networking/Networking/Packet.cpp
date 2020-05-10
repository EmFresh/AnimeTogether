#include "Packet.h"

Packet::Packet(PacketType type)
{
	clear();
	assignPacketType(type);
}

PacketType Packet::getPacketType()
{
	PacketType* PTPointer = reinterpret_cast<PacketType*>(&buffer[0]);
	return static_cast<PacketType>(ntohs(*PTPointer));
}

void Packet::assignPacketType(PacketType type)
{
	PacketType* PTPointer = reinterpret_cast<PacketType*>(&buffer[0]);
	*PTPointer = static_cast<PacketType>( htons(type));
}

void Packet::clear()
{
	buffer.resize(sizeof(PacketType));
	assignPacketType(PT_Invalid);
	extractOffset = sizeof(PacketType);
}

void Packet::append(const void* data, uint32_t size)
{
	if(buffer.size() + size > MAX_PACKET_SIZE)
	{
		throw PacketError("[Packet::append(const void* data, uint32_t size)] - Packet size exceeded MAX_PACKET_SIZE");
		return;
	}

	buffer.insert(buffer.end(), (char*)data, (char*)data + size);
}

Packet& Packet::operator<<(uint32_t data)
{
	data = htonl(data);
	append(&data, sizeof(uint32_t));
	return *this;
	// TODO: insert return statement here
}

Packet& Packet::operator>>(uint32_t& data)
{
	if((extractOffset + sizeof(uint32_t)) > buffer.size())
		throw PacketError("[Packet::opperator>>(uint32_t& size)] - Extraction offset exceeded buffer size.");

	data = *reinterpret_cast<uint32_t*> (&buffer[extractOffset]);
	data = ntohl(data);
	extractOffset += sizeof(uint32_t);

	return *this;
}

Packet& Packet::operator<<(const std::string data)
{
	*this << (uint32_t)data.size();
	append(data.data(),(uint32_t) data.size());
	return *this;
}

Packet& Packet::operator>>(std::string& data)
{
	data.clear();

	uint32_t stringSize = 0;
	*this >> stringSize;

	if((extractOffset + stringSize) > buffer.size())
		throw PacketError("[Packet::opperator>>(std::string&)] - Extraction offset exceeded buffer size.");

	data.resize(stringSize);
	data.assign(&buffer[extractOffset], stringSize);
	extractOffset += stringSize;

	return *this;
}
