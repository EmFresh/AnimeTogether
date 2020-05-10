#include "LastNetworkError.h"
#include <WinSock2.h>
#include <system_error>
std::string LastNetworkError::last = "";

std::string& LastNetworkError::GetLastError()
{
	static std::string tmp; tmp = last;
	return last.clear(), tmp;
}

void LastNetworkError::SetLastError(const char* head, int code)
{
	
	last = std::string(head) + std::system_category().message(code);//example: "Socket error: Invalid socket used."
	

}
