#include "LastNetworkError.h"
#include <WinSock2.h>
std::string LastNetworkError::last = "";

std::string& LastNetworkError::GetLastError()
{
	static std::string tmp; tmp = last;
	return last.clear(), tmp;
}

void LastNetworkError::SetLastError(const char* head, int code)
{
	void* lpMsgBuf = nullptr;
	FormatMessage(
		FORMAT_MESSAGE_ALLOCATE_BUFFER |
		FORMAT_MESSAGE_FROM_SYSTEM |
		FORMAT_MESSAGE_IGNORE_INSERTS,
		NULL,
		(DWORD)code,
		MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
		(LPTSTR)&lpMsgBuf,
		0, NULL);

	last = std::string(head) + (code >= 0 ? (const char*)lpMsgBuf : "");//example: "Socket error: Invalid socket used."
}
