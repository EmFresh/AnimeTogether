#pragma once

#include <string>

class PacketError
{
public:
	PacketError(std::string exep):m_exep(exep){}

	const char* what()
	{
		return m_exep.c_str();
	}

	std::string toString()
	{
		return m_exep;
	}
private:
	std::string m_exep;
};

