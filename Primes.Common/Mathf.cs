using System;

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
			if (number < 2)
				return false;

			if (number < 4)
				return true;

			if ((number % 2) == 0)
				return false;

			ulong current = 5, sqrt = UlongSqrtHigh(number);

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
					current += 2;

					if ((current % 2) == 0)
						current--;
				}

				if (number % current == 0)
					return false;
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
			if (number < 2)
				return number;

			ulong max = 4294967295, min = 0, c, c2;

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



		/// <summary>
		/// Clamps a value between a minimum and a maximum.
		/// </summary>
		/// <param name="value">The number to be clamped.</param>
		/// <param name="min">Minimum final value.</param>
		/// <param name="max">Maximum final value.</param>
		/// <returns></returns>
		public static byte Clamp(byte value, byte min, byte max)
		{
			value = value > max ? max : value;

			return value < min ? min : value;
		}
		/// <summary>
		/// Clamps a value between a minimum and a maximum.
		/// </summary>
		/// <param name="value">The number to be clamped.</param>
		/// <param name="min">Minimum final value.</param>
		/// <param name="max">Maximum final value.</param>
		/// <returns></returns>
		public static sbyte Clamp(sbyte value, sbyte min, sbyte max)
		{
			value = value > max ? max : value;

			return value < min ? min : value;
		}
		/// <summary>
		/// Clamps a value between a minimum and a maximum.
		/// </summary>
		/// <param name="value">The number to be clamped.</param>
		/// <param name="min">Minimum final value.</param>
		/// <param name="max">Maximum final value.</param>
		/// <returns></returns>
		public static ushort Clamp(ushort value, ushort min, ushort max)
		{
			value = value > max ? max : value;

			return value < min ? min : value;
		}
		/// <summary>
		/// Clamps a value between a minimum and a maximum.
		/// </summary>
		/// <param name="value">The number to be clamped.</param>
		/// <param name="min">Minimum final value.</param>
		/// <param name="max">Maximum final value.</param>
		/// <returns></returns>
		public static short Clamp(short value, short min, short max)
		{
			value = value > max ? max : value;

			return value < min ? min : value;
		}
		/// <summary>
		/// Clamps a value between a minimum and a maximum.
		/// </summary>
		/// <param name="value">The number to be clamped.</param>
		/// <param name="min">Minimum final value.</param>
		/// <param name="max">Maximum final value.</param>
		/// <returns></returns>
		public static uint Clamp(uint value, uint min, uint max)
		{
			value = value > max ? max : value;

			return value < min ? min : value;
		}
		/// <summary>
		/// Clamps a value between a minimum and a maximum.
		/// </summary>
		/// <param name="value">The number to be clamped.</param>
		/// <param name="min">Minimum final value.</param>
		/// <param name="max">Maximum final value.</param>
		/// <returns></returns>
		public static int Clamp(int value, int min, int max)
        {
			value = value > max ? max : value;

			return value < min ? min : value;
        }
		/// <summary>
		/// Clamps a value between a minimum and a maximum.
		/// </summary>
		/// <param name="value">The number to be clamped.</param>
		/// <param name="min">Minimum final value.</param>
		/// <param name="max">Maximum final value.</param>
		/// <returns></returns>
		public static long Clamp(long value, long min, long max)
		{
			value = value > max ? max : value;

			return value < min ? min : value;
		}
		/// <summary>
		/// Clamps a value between a minimum and a maximum.
		/// </summary>
		/// <param name="value">The number to be clamped.</param>
		/// <param name="min">Minimum final value.</param>
		/// <param name="max">Maximum final value.</param>
		/// <returns></returns>
		public static ulong Clamp(ulong value, ulong min, ulong max)
		{
			value = value > max ? max : value;

			return value < min ? min : value;
		}
	}
}
