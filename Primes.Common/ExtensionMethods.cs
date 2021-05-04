using System;

namespace Primes.Common
{
    /// <summary>
    /// Class that contains several extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Truncates the string to a given length.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength">The length to truncate the string to.</param>
        /// <returns>Truncated string.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
        /// <summary>
        /// Returns a string with the given length, either truncated or padded with spaces.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="length">The length to set the string to.</param>
        /// <returns>Rescaled string.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static string SetLength(this string value, int length)
        {
            if (value.Length >= length)
                return value.Substring(0, length);

            return value += " ".Loop(length - value.Length);
        }
        /// <summary>
        /// Repeats the string a given amount of times.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="times">The number of times to repeat the string.</param>
        /// <returns>Looped string.</returns>
        public static string Loop(this string value, int times)
        {
            string f = string.Empty;

            for (int i = 0; i < times; i++) f += value;

            return f;
        }
        /// <summary>
        /// Gets the first X values from an array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="amount">The number of values to return.</param>
        /// <returns></returns>
        public static T[] GetFirst<T>(this T[] array, int amount)
        {
            T[] ret = new T[amount];

            Array.Copy(array, ret, amount);

            return ret;
        }
    }
}
