﻿using System;

namespace Primes.Common
{
	/// <summary>
	/// Class that contains several math-related methods.
	/// </summary>
    public static class Mathf
    {
		/// <summary>
		/// Checks if a number is prime or not, using PeakRead's method.
		/// </summary>
		/// <param name="number">The number to be checked.</param>
		/// <returns>True if the number is prime, false otherwise.</returns>
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
		/// <summary>
		/// Checks if a number is prime or not, based on previously checked numbers, using Didas72's method.
		/// </summary>
		/// <param name="number">The number to be checked.</param>
		/// <param name="knownPrimes">A reference to an array of known prime numbers.</param>
		/// <returns>True if the number is prime, false otherwise.</returns>
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



		/// <summary>
		/// Calculates the closest integer to the square root of the given number, always rounding upwards.
		/// </summary>
		/// <param name="number">The number whose square root is wanted.</param>
		/// <returns>Upwards-rounded square root.</returns>
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



		/// <summary>
		/// Adds two <see cref="ulong"/> values, throwing an <see cref="OverflowException"/> if relevant.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns>Sum of the give numbers.</returns>
		public static ulong SafeAdd(ulong a, ulong b)
		{
			if (a / 2 + b / 2 > ulong.MaxValue / 2)
				throw new OverflowException();

			return a + b;
		}



		/// <summary>
		/// Calculates 10 to the power of exp.
		/// </summary>
		/// <param name="exp"></param>
		/// <returns></returns>
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
		/// <summary>
		/// Calculates s time 10 to the power of exp.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="exp"></param>
		/// <returns></returns>
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
