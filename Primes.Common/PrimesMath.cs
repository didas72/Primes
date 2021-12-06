using System;

using DidasUtils;

namespace Primes.Common
{
	/// <summary>
	/// Class that contains several math-related methods.
	/// </summary>
    public static class PrimesMath
    {
		/// <summary>
		/// Checks if a number is prime or not, using PeakRead's method.
		/// </summary>
		/// <param name="number">The number to be checked.</param>
		/// <returns>True if the number is prime, false otherwise.</returns>
		public static bool IsPrime(ulong number) => IsPrime(number, 5, out _);
		/// <summary>
		/// Checks if a number is prime or not, using PeakRead's method.
		/// </summary>
		/// <param name="number">The number to be checked.</param>
		/// <param name="divider">The divider of the number checked if it is not prime, 0 otherwise.</param>
		/// <returns>True if the number is prime, false otherwise.</returns>
		public static bool IsPrime(ulong number, out ulong divider) => IsPrime(number, 5, out divider);
		/// <summary>
		/// Checks if a number is prime or not, using PeakRead's method.
		/// </summary>
		/// <param name="number">The number to be checked.</param>
		/// <param name="current">The last number checked against.</param>
		/// <returns>True if the number is prime, false otherwise.</returns>
		public static bool IsPrime(ulong number, ulong current) => IsPrime(number, current, out _);
		/// <summary>
		/// Checks if a number is prime or not, using PeakRead's method.
		/// </summary>
		/// <param name="number">The number to be checked.</param>
		/// <param name="current">The last number checked against.</param>
		/// <param name="divider">The divider of the number checked if it is not prime, 0 otherwise.</param>
		/// <returns>True if the number is prime, false otherwise.</returns>
		public static bool IsPrime(ulong number, ulong current, out ulong divider)
		{
			divider = 0;

			if (number < 2)
				return false;

			if (number < 4)
				return true;

			if ((number % 2) == 0)
            {
				divider = 2;
				return false;
			}

			current = Mathf.Clamp(current, 5, ulong.MaxValue);
			ulong sqrt = UlongSqrtHigh(number);

			while (current <= sqrt)
			{
				if (number % current == 0)
                {
					divider =  current;
					return false;
				}

				current += 2;
			}

			divider = 0;
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
			if (number < 2)
				return false;

			if (number < 4)
				return true;

			if ((number % 2) == 0)
				return false;

			int i = 0;
			ulong current = 5, sqrt = UlongSqrtHigh(number);

			while (current < sqrt)
			{
				if (i < knownPrimes.Length)
				{
					current = knownPrimes[i++];
				}
				else //if we run out of primes
				{
					if ((current % 2) == 0)
						current--;

					return IsPrime(number, current, out _);
				}

				if (number % current == 0)
					return false;
			}

			return true;
		}
		/// <summary>
		/// Checks if a number is prime or not, based on previously checked numbers, using Didas72's method.
		/// </summary>
		/// <param name="number">The number to be checked.</param>
		/// <param name="knownPrimes">A reference to an array of known prime numbers.</param>
		/// <param name="divider">The divider of the number checked if it is not prime, 0 otherwise.</param>
		/// <returns>True if the number is prime, false otherwise.</returns>
		public static bool IsPrime(ulong number, ref ulong[] knownPrimes, out ulong divider)
		{
			divider = 0;

			if (number < 2)
				return false;

			if (number < 4)
				return true;

			if ((number % 2) == 0)
            {
				divider = 2;
				return false;
			}

			int i = 0;
			ulong current = 5, sqrt = UlongSqrtHigh(number);

			while (current < sqrt)
			{
				if (i < knownPrimes.Length)
				{
					current = knownPrimes[i++];
				}
				else //if we run out of primes
				{
					if ((current % 2) == 0)
						current--;

					return IsPrime(number, current, out divider);
				}

				if (number % current == 0)
				{
					divider = current;
					return false;
				}
			}

			return true;
		}



		/// <summary>
		/// Calculates the closest integer to the square root of the given number, always rounding upwards.
		/// </summary>
		/// <param name="number">The number whose square root is wanted.</param>
		/// <returns>Upwards-rounded square root.</returns>
		public static ulong UlongSqrtHigh(ulong number)
		{
			if (number <= 2)
				return number;

			ulong max = 4294967296, min = 0, c, c2;

			while (true)
			{
				c = (max + min) / 2;
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
	}
}
