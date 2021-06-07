// PrimesCommon.cpp : Defines the exported functions for the DLL.

#include "Primes.Common.h"
#include "ShortTypeNames.h"

namespace Primes
{
	namespace Common
	{
		namespace Math
		{
			ulong UlongSqrtHigh(const ulong& number)
			{
				if (number < 3)
					return number;

				ulong max = 4294967295, min = 1, c, c2;

				while (true)
				{
					c = min + ((max - min) / 2);
					c2 = c * c;

					if (c2 < number)
						min = c;
					else if (c2 > number)
						max = c;
					else
						return c;

					if (max - min <= 1)
						return max;
				}
			}

			bool IsPrime(const ulong& number)
			{
				if (number < 2)
					return false;

				if (number < 4)
					return true;

				if ((number % 2) == 0)
					return false;

				ulong current = 5, sqrt = UlongSqrtHigh(number);

				while (current <= sqrt)
				{
					if ((number % current) == 0)
						return false;

					current += 2;
				}

				return true;
			}

			bool IsPrime(const ulong& number, const ulong* knownPrimes, const uint& KPLength)
			{
				if (number < 2)
					return false;

				if (number < 4)
					return true;

				if ((number % 2) == 0)
					return false;

				unsigned int i = 0;
				ulong current = 5, sqrt = UlongSqrtHigh(number);

				while (current <= sqrt)
				{
					if (i < KPLength)
					{
						current = *(knownPrimes + i);
						i++;
					}
					else
					{
						current += 2;

						if ((current % 2) == 0)
							current--;
					}

					if ((number % current) == 0)
						return false;
				}

				return true;
			}
		}
	}
}
