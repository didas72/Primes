//ConsoleIO.cpp - Includes functions related to IO to the console

#include <iostream>

void Printf(std::string msg)
{
	std::cout << msg << std::endl;
}

std::string ReadLine()
{
	std::string out;

	std::cin >> out;

	return out;
}
