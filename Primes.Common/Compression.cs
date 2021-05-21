using System;
using System.Collections.Generic;

namespace Primes.Common.Files
{
    /// <summary>
    /// Class that contains several compression-related methods. Made originally in java by PeakRead, adapted by Didas72.
    /// </summary>
    public static class Compression
    {
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
}
