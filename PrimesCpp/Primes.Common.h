// PrimesComon.h - Contains declarations of primes functions
#pragma once

#include "ShortTypeNames.h"

namespace Primes
{
	namespace Common
	{
		namespace Math
		{
			ulong UlongSqrtHigh(const ulong& number);

			bool IsPrime(const ulong& number);

			bool IsPrime(const ulong& number, const ulong* knownPrimes, const uint& KPLength);
		}
	}
}
