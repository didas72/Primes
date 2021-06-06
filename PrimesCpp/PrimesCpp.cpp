// PrimesCpp.cpp : This file contains the 'main' function. Program execution begins and ends there.

#include <iostream>
#include <string>
#include <chrono>

#include "PrimesCpp.h"
#include "PrimesCommon.h"
#include "ConsoleIO.h"

#define ulong unsigned long long

using namespace std;

int main()
{
	using std::chrono::high_resolution_clock;
	using std::chrono::duration_cast;
	using std::chrono::duration;
	using std::chrono::milliseconds;

	ulong a;

	auto t1 = high_resolution_clock::now();
	
	for (ulong i = 1; i < 100001; i++)
	{
		/*Printf(std::to_string(*/a = UlongSqrtHigh(i * 163 - 3)/*))*/;
	}

	auto t2 = high_resolution_clock::now();

	duration<double, std::milli> ms_double = t2 - t1;

	Printf(std::to_string(a));

	Printf(std::to_string(ms_double.count()));

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
