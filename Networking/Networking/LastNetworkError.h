#pragma once
#include <string>
class LastNetworkError
{
public:
	static std::string& GetLastError();
	static void SetLastError(const char* head, int code = -1);
private:
	static std::string last;
};