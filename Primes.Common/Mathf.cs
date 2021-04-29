using System;

namespace Primes.Common
{
    public static class Mathf
    {
		public static bool IsPrime(ulong number)
		{
			if (number == 2 || number == 3)
				return true;

			ulong current = 3;
			ulong sqrt = UlongSqrtHigh(number);

			while (current <= sqrt)
			{
				if (number % current == 0)
					return false;

				current += 2;
			}

			return true;
		}
		public static bool IsPrime(ulong number, ref ulong[] knownPrimes)
		{
			int i = 0;
			ulong knownPrime = 3;
			ulong sqrt = UlongSqrtHigh(number);

			do
			{
				if (i < knownPrimes.Length)
				{
					knownPrime = knownPrimes[i++];
				}
				else //if we run out of primes
				{
					knownPrime += 2;
				}

				if (number % knownPrime == 0)
					return false;
			}
			while (knownPrime < sqrt);

			return true;
		}



		public static ulong UlongSqrtHigh(ulong number)
		{
			if (number < 1)
				throw new ArgumentException(number + " is smaller than 1, the minimum value for the integer square root.");

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



		public static ulong SafeAdd(ulong a, ulong b)
		{
			if (a / 2 + b / 2 > ulong.MaxValue / 2)
				throw new OverflowException();

			return a + b;
		}



		public static ulong Exp(byte exp)
		{
			ulong s = 1;

			while (exp > 0)
			{
				s *= 10;
				exp--;
			}

			return s;
		}
		public static ulong Exp(ulong s, byte exp)
		{
			while (exp > 0)
			{
				s *= 10;
				exp--;
			}

			return s;
		}
	}
}
