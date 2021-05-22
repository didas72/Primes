using System;
using System.Collections.Generic;

namespace Primes.Common.Files
{
    /// <summary>
    /// Class that contains several compression related methods.
    /// </summary>
    public static class Compression
    {
        /// <summary>
        /// Class that contains all methods related to Optimized Numerical Sequence Storage compression.
        /// These methods were originally made by PeakRead and adapted to C# by Didas72.
        /// </summary>
        public static class ONSS
        {
            /// <summary>
            /// Compresses an <see cref="ulong"/> array using ONSS.
            /// </summary>
            /// <param name="ulongs">The ulong array to compress</param>
            /// <returns>Byte array with the compressed ulongs</returns>
            public static byte[] Compress(ulong[] ulongs)
            {
                if (ulongs.Length < 2)
                    throw new ArgumentException();

                List<byte> bytes = new List<byte>();

                //first is always raw

                bytes.AddRange(BitConverter.GetBytes(ulongs[0]));

                //actual encoding now

                int header = 1;
                ulong reference = ulongs[0];

                while (header < ulongs.Length)
                {
                    ulong delta = ulongs[header] - reference;

                    if (delta > 0xFFFF)
                    {
                        bytes.AddRange(new byte[] { 0, 0 });
                        bytes.AddRange(BitConverter.GetBytes(ulongs[header]));

                        reference = ulongs[header];

                        header++;
                    }
                    else
                    {
                        bytes.AddRange(BitConverter.GetBytes((ushort)delta));

                        header++;
                    }
                }

                return bytes.ToArray();
            }



            /// <summary>
            /// Uncompresses an <see cref="ulong"/> array using ONSS.
            /// </summary>
            /// <param name="bytes">The byte array to uncompress.</param>
            /// <returns>Ulong array with the uncompressed values.</returns>
            public static ulong[] Uncompress(byte[] bytes)
            {
                if (bytes.Length < 8)
                    throw new ArgumentException();

                List<ulong> ulongs = new List<ulong>();

                //first 8 bytes always first value

                ulong reference = BitConverter.ToUInt64(bytes, 0);

                ulongs.Add(reference);

                //actual decoding now

                int header = 8;

                while (header < bytes.Length)
                {
                    if (bytes[header] == 0 && bytes[header + 1] == 0)
                    {
                        header += 2;

                        ulong value = BitConverter.ToUInt64(bytes, header);

                        ulongs.Add(value);
                        reference = value;

                        header += 8;
                    }
                    else
                    {
                        ushort delta = BitConverter.ToUInt16(bytes, header);

                        ulongs.Add(reference + delta);

                        header += 2;
                    }
                }

                return ulongs.ToArray();
            }
        }



        /// <summary>
        /// Class that contains all methods related to Numerical Chain Compression.
        /// </summary>
        public static class NCC
        {
            /// <summary>
            /// Compresses an <see cref="ulong"/> array using NCC.
            /// </summary>
            /// <param name="ulongs">The ulong array to compress</param>
            /// <returns>Byte array with the compressed ulongs</returns>
            public static byte[] Compress(ulong[] ulongs)
            {
                if (ulongs.Length < 2)
                    throw new ArgumentException();

                List<byte> bytes = new List<byte>();

                //first is always raw

                bytes.AddRange(BitConverter.GetBytes(ulongs[0]));

                //actual encoding now

                int header = 1;

                while (header < ulongs.Length)
                {
                    ulong delta = ulongs[header] - ulongs[header - 1];

                    if (delta > 0xFFFF)
                    {
                        bytes.AddRange(new byte[] { 0, 0 });
                        bytes.AddRange(BitConverter.GetBytes(ulongs[header]));

                        header++;
                    }
                    else
                    {
                        bytes.AddRange(BitConverter.GetBytes((ushort)delta));

                        header++;
                    }
                }

                return bytes.ToArray();
            }



            /// <summary>
            /// Uncompresses an <see cref="ulong"/> array using NCCS.
            /// </summary>
            /// <param name="bytes">The byte array to uncompress.</param>
            /// <returns>Ulong array with the uncompressed values.</returns>
            public static ulong[] Uncompress(byte[] bytes)
            {
                if (bytes.Length < 8)
                    throw new ArgumentException();

                List<ulong> ulongs = new List<ulong>();

                //first 8 bytes always first value

                ulong last = BitConverter.ToUInt64(bytes, 0);

                ulongs.Add(last);

                //actual decoding now

                int header = 8;

                while (header < bytes.Length)
                {
                    if (bytes[header] == 0 && bytes[header + 1] == 0)
                    {
                        header += 2;

                        ulong value = BitConverter.ToUInt64(bytes, header);

                        ulongs.Add(value);
                        last = value;

                        header += 8;
                    }
                    else
                    {
                        ushort delta = BitConverter.ToUInt16(bytes, header);

                        last += delta;

                        ulongs.Add(last);

                        header += 2;
                    }
                }

                return ulongs.ToArray();
            }
        }



        /// <summary>
        /// Exception thrown when <see cref="PrimeJob.Comp"/> represent an invalid compression method.
        /// </summary>
        public class InvalidCompressionMethodException : Exception
        {
            /// <summary>
            /// Initializes a new instance of <see cref="InvalidCompressionMethodException"/> with no message.
            /// </summary>
            public InvalidCompressionMethodException() : base() { }
            /// <summary>
            /// Initializes a new instance of <see cref="InvalidCompressionMethodException"/> with the given message.
            /// </summary>
            /// <param name="message"></param>
            public InvalidCompressionMethodException(string message) : base(message) { }
        }
    }
}
