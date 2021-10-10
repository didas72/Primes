using System;
using System.Linq;

namespace Primes.Common.ErrorCorrection
{
    /// <summary>
    /// Class that holds methods to assist with the creation of fletcher checksums.
    /// </summary>
    /// <remarks>Adapted from Wikipedia's article on Fletcher's Checksum on 10/10/2021</remarks>
    public static class Fletcher
    {
        /// <summary>
        /// Calculates the 16-bit Flether checksum for the given source bytes.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static ushort Fletcher16(byte[] source)
        {
            uint c0 = 0, c1 = 0;
            int len = source.Length;

            for (int i = 0; len > 0;)
            {
                int blocklen = len;

                if (blocklen > 5002)
                    blocklen = 5002;

                len -= blocklen;

                do
                {
                    c0 += source[i++];
                    c1 += c0;
                } while (--blocklen > 0);

                c0 %= 255;
                c1 %= 255;
            }
            return (ushort)(c1 << 8 | c0);
        }
        /// <summary>
        /// Calculates the 32-bit Flether checksum for the given source bytes.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static uint Fletcher32(byte[] source)
        {
            if ((source.Length % 2) != 0)
                source = source.Append<byte>(0).ToArray();

            ushort[] newSource = new ushort[Mathf.DivideRoundUp(source.Length, 2)];
            Buffer.BlockCopy(source, 0, newSource, 0, source.Length);

            return Fletcher32(newSource);
        }
        /// <summary>
        /// Calculates the 32-bit Flether checksum for the given source uhsorts.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static uint Fletcher32(ushort[] source){

	        uint c0 = 0, c1 = 0;
            int len = (source.Length + 1) & ~1;

	        for (int i = 0; len > 0; )
            {
		        int blocklen = len;

		        if (blocklen > 360 * 2)
                    blocklen = 360 * 2;

                len -= blocklen;

		        do
                {
                    c0 += source[i++];
			        c1 += c0;
                } while ((blocklen -= 2) > 0);

                c0 %= 65535;
                c1 %= 65535;
	        }

	        return (c1 << 16 | c0);
        }
    }
}
