namespace Primes.Common.ErrorCorrection
{
    /// <summary>
    /// Class that holds methods to assist with the creation of checksums.
    /// </summary>
    public static class CheckSum
    {
        /// <summary>
        /// Calculates the 64-bit checksum of the given values.
        /// </summary>
        /// <param name="source">The values to calculate the checksum of.</param>
        /// <returns></returns>
        public static ulong CheckSum64(ulong[] source)
        {
            ulong checkSum = 0;

            for (int i = 0; i < source.Length; i++)
            {
                checkSum += source[i];
            }

            return ulong.MaxValue - checkSum;
        }
        /// <summary>
        /// Calculates the 32-bit checksum of the given values.
        /// </summary>
        /// <param name="source">The values to calculate the checksum of.</param>
        /// <returns></returns>
        public static uint CheckSum32(uint[] source)
        {
            uint checkSum = 0;

            for (int i = 0; i < source.Length; i++)
            {
                checkSum += source[i];
            }

            return uint.MaxValue - checkSum;
        }
        /// <summary>
        /// Calculates the 16-bit checksum of the given values.
        /// </summary>
        /// <param name="source">The values to calculate the checksum of.</param>
        /// <returns></returns>
        public static ushort CheckSum16(ushort[] source)
        {
            ushort checkSum = 0;

            for (int i = 0; i < source.Length; i++)
            {
                checkSum += source[i];
            }

            return (ushort)(ushort.MaxValue - checkSum);
        }
        /// <summary>
        /// Calculates the 8-bit checksum of the given values.
        /// </summary>
        /// <param name="source">The values to calculate the checksum of.</param>
        /// <returns></returns>
        public static byte CheckSum8(byte[] source)
        {
            byte checkSum = 0;

            for (int i = 0; i < source.Length; i++)
            {
                checkSum += source[i];
            }

            return (byte)(byte.MaxValue - checkSum);
        }
    }
}
