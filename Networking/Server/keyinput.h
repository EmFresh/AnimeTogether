/*************************************************
**Name : Emory Wynn								**
**Student# : 100655604							**
**note : please run this aplication in debug	**
**************************************************/

#pragma once
#include <Windows.h>
#include <unordered_map>
#include <vector>
#include <string>

/*----User defined classes----*/
#define amount (wait2 == 0 ? 200 : 25)

class KeyInput
{
private:
	static std::unordered_map<int, bool>enter;
	static std::vector<char>
		numShiftKeys,
		symbalKeys,
		symbalShiftKeys,
		operators;
	static short count, count2, wait, wait2, pressed, length;
	static bool typed;
	static std::string typing;

public:
	void setTyped(std::string str);

	/*Sets the longest length of the input.
	If the length is less than, 0 no limit is placed*/
	void setTypedLength(int l);

	static void clearType();

	static int getTypedSize();

	//Async typing
	static const char* type();

	/*
	 bool stroke(int key);
	 * key - The key which is pressed. You can either
	 use VK_KEYS (i.e. VK_RIGHT) or characters
	 (Note: characters must be in uppercase)

	 to be checked if key is pressed and then released
	*/
	static bool stroke(int key);

	/*
	bool press(int key)
	* key - The key which is pressed. You can either
	use VK_KEYS (i.e. VK_RIGHT) or characters
	(Note: characters must be in uppercase) to be
	checked if key is pressed
	*/
	static bool press(int key);

	/*
	bool release(int key)
	* key - The key which is pressed. You can either
	use VK_KEYS (i.e. VK_RIGHT) or characters
	(Note: characters must be in uppercase) to be
	checked if key is released
	*/
	static bool release(int key);
};
