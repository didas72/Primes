// PrimesCpp.cpp : This file contains the 'main' function. Program execution begins and ends there.

#include <iostream>
#include <string>

#include "PrimesCpp.h"
#include "Primes.Common.h"
#include "ConsoleIO.h"
#include "ShortTypeNames.h"

using namespace std;

int main()
{


	return 0;
}

ulong GetUlongInput()
{
	string read = ReadLine();

	try
	{
		ulong val = stoi(read);
		return val;
	}
	catch (...)
	{
		return GetUlongInput();
	}
}
