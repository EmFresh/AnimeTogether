/*************************************************
**Name : Emory Wynn								**
**Student# : 100655604							**
**note : please run this aplication in debug	**
**************************************************/

#include "KeyInput.h"


std::unordered_map<int, bool> KeyInput::enter;
std::vector<char>
KeyInput::numShiftKeys{')','!','@','#','$','%','^','&','*','('},
KeyInput::symbalKeys{';','/','`','[','\\',']','\'','=',',','-','.'},
KeyInput::symbalShiftKeys{':','?','~','{','|','}','\"','+','<','_','>'},
KeyInput::operators{'*','+','-','.','/'};
short KeyInput::count, KeyInput::count2, KeyInput::wait, KeyInput::wait2, KeyInput::pressed, KeyInput::length = -1;
bool KeyInput::typed;
std::string KeyInput::typing;

void KeyInput::setTyped(std::string str)
{
	typing = str;
}

void KeyInput::setTypedLength(int l)
{
	length = l;
}

void KeyInput::clearType()
{
	typing = "";
}

int KeyInput::getTypedSize()
{
	return typing.size();
}

//async typing
const char* KeyInput::type()
{
	//OutputDebugStringA((std::to_string(count) + "\n").c_str());
	if(!press(pressed) && pressed != 0)
		count2 = wait2;

	if((length > -1 ? typing.size() < (unsigned)length : false))
	{
		if(count2++ >= wait2)
		{
			count2 = 0;


			//numpad nums & regular nums
			for(short a = 0x30; a - 0x30 < 10; a++)
			{
				if(press(pressed = a) || (press(pressed = a + 48) && GetKeyState(VK_NUMLOCK)))
					if(!press(VK_SHIFT))
					{
						wait2 = amount;
						return (typing += (char)a).c_str();
					} else
					{
						wait2 = amount;
						return (typing += numShiftKeys[a - 0x30]).c_str();
					}
			}
			//numpad operators
			for(short a = 0x6A; a - 0x6A < 2; a++)
			{
				if(press(pressed = a))
					if(GetKeyState(VK_NUMLOCK))
					{
						wait2 = amount;
						return (typing += operators[a - 0x6A]).c_str();
					}
			}
			for(short a = 0x6D; a - 0x6D < 3; a++)
			{
				if(press(pressed = a))
					if(GetKeyState(VK_NUMLOCK))
					{
						wait2 = amount;
						return (typing += operators[a - 0x6D + 2]).c_str();
					}
			}
			//letters
			for(short a = 0x41; a - 0x41 < 26; a++)
			{
				if(press(pressed = a))
					if(press(VK_SHIFT) || GetKeyState(VK_CAPITAL))
					{
						wait2 = amount;
						return (typing += (char)a).c_str();
					} else
					{
						wait2 = amount;
						return (typing += a + 32).c_str();
					}
			}
			//OEM (dual use buttons)		
			if(press(pressed = 0xBA))
			{
				if(press(VK_SHIFT))
				{
					wait2 = amount;
					return (typing += symbalShiftKeys[0]).c_str();
				} else
				{
					wait2 = amount;
					return (typing += symbalKeys[0]).c_str();
				}
			}
			for(short a = 0xBF; a - 0xBF < 2; a++)
			{
				if(press(pressed = a))
					if(press(VK_SHIFT))
					{
						wait2 = amount;
						return (typing += symbalShiftKeys[a - 0xBF + 1]).c_str();
					} else
					{
						wait2 = amount;
						return (typing += symbalKeys[a - 0xBF + 1]).c_str();
					}
			}
			for(short a = 0xDB; a - 0xDB < 4; a++)
			{
				if(press(pressed = a))
					if(press(VK_SHIFT))
					{
						wait2 = amount;
						return (typing += symbalShiftKeys[a - 0xDB + 3]).c_str();
					} else
					{
						wait2 = amount;
						return (typing += symbalKeys[a - 0xDB + 3]).c_str();
					}
			}
			for(short a = 0xBB; a - 0XBB < 4; a++)
			{
				if(press(pressed = a))
					if(press(VK_SHIFT))
					{
						wait2 = amount;
						return (typing += symbalShiftKeys[a - 0XBB + 7]).c_str();
					} else
					{
						wait2 = amount;
						return (typing += symbalKeys[a - 0XBB + 7]).c_str();
					}
			}
			wait2 = 0;
			pressed = 0;
		}
	} else
	{
		for(int x = 0; x < 256; x++)
			(char)(GetAsyncKeyState(x) >> 8);
	}

	if(count++ == wait)
	{
		count = 0;
		if(!typing.empty() && press(VK_BACK))
		{
			wait = (wait == 0 ? 200 : 25);
			return (typing = typing.substr(0, typing.size() - 1)).c_str();
		} else if(stroke(VK_SPACE))
			return (typing += ' ').c_str();
		wait = 0;
	}

	return typing.c_str();
}

/*
bool stroke(int key);
* key - The key which is pressed. You can either
use VK_KEYS (i.e. VK_RIGHT) or characters
(Note: characters must be in uppercase)

to be checked if key is pressed and then released
*/
bool KeyInput::stroke(int key)
{

	if(GetAsyncKeyState(toupper(key)))
		enter[toupper(key)] = true;
	else if(enter[toupper(key)] && !GetAsyncKeyState(toupper(key)))
	{
		enter[toupper(key)] = false;
		return true;
	}
	return false;
}

/*
bool press(int key)
* key - The key which is pressed. You can either
use VK_KEYS (i.e. VK_RIGHT) or characters
(Note: characters must be in uppercase) to be
checked if key is pressed
*/
bool KeyInput::press(int key)
{
	return GetAsyncKeyState(toupper(key));
}

/*
bool release(int key)
* key - The key which is pressed. You can either
use VK_KEYS (i.e. VK_RIGHT) or characters
(Note: characters must be in uppercase) to be
checked if key is released
*/
bool KeyInput::release(int key)
{
	return !GetAsyncKeyState(toupper(key));
}