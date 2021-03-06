using System;
using System.Collections.Generic;

using Primes.Common.Files;

namespace Primes.Common.Serializers
{
    /// <summary>
    /// Class that contains several <see cref="PrimeJob"/> serialization related methods.
    /// </summary>
    public static class PrimeJobSerializer
    {
        //Version Definitions
        /*  v1.0.0
         *  0   3 bytes     Version     version (1 byte major, 1 byte minor, 1 byte patch)
         *  3   8 bytes     ulong       start
         *  11  8 bytes     ulong       count
         *  19  8 bytes     ulong       progress
         *  27  4 bytes     int         primesInFile
         *  31  xxx         ulong[]     primes
         */
        /*  v1.1.0
         *  0   3 bytes     Version     version (1 byte major, 1 byte minor, 1 byte patch)
         *  3   4 bytes     uint        batch
         *  7   8 bytes     ulong       start
         *  15  8 bytes     ulong       count
         *  23  8 bytes     ulong       progress
         *  31  4 bytes     int         primesInFile
         *  35  xxx         ulong[]     primes
         */
        /*  v1.2.0
         *  0   3 bytes     Version     version (1 byte major, 1 byte minor, 1 byte patch)
         *  3   1 byte      Comp        compression header (0b000000, 1 bit NNC, 1 bit ONSS)
         *  4   4 bytes     uint        batch
         *  8   8 bytes     ulong       start
         *  16  8 bytes     ulong       count
         *  24  8 bytes     ulong       progress
         *  32  xxx         ulong[]     (compressed) primes
         */



        /// <summary>
        /// Deserializes a <see cref="PrimeJob"/> of version 1.0.0 from a byte array.
        /// </summary>
        /// <param name="bytes">Byte array contaning the serialized <see cref="PrimeJob"/>.</param>
        /// <returns></returns>
        public static PrimeJob Deserializev1_0_0(ref byte[] bytes)
        {
            ulong start = BitConverter.ToUInt64(bytes, 3);
            ulong count = BitConverter.ToUInt64(bytes, 11);
            ulong progress = BitConverter.ToUInt64(bytes, 19);

            int primesInFile = BitConverter.ToInt32(bytes, 27);

            ulong[] primes = new ulong[primesInFile];
            Buffer.BlockCopy(bytes, 31, primes, 0, primesInFile * 8);

            return new PrimeJob(new PrimeJob.Version(1, 0, 0), start, count, progress, ref primes);
        }
        /// <summary>
        /// Deserializes a <see cref="PrimeJob"/> of version 1.1.0 from a byte array.
        /// </summary>
        /// <param name="bytes">Byte array contaning the serialized <see cref="PrimeJob"/>.</param>
        /// <returns></returns>
        public static PrimeJob Deserializev1_1_0(ref byte[] bytes)
        {
            uint batch = BitConverter.ToUInt32(bytes, 3);
            ulong start = BitConverter.ToUInt64(bytes, 7);
            ulong count = BitConverter.ToUInt64(bytes, 15);
            ulong progress = BitConverter.ToUInt64(bytes, 23);

            int primesInFile = BitConverter.ToInt32(bytes, 31);

            ulong[] primes = new ulong[primesInFile];
            Buffer.BlockCopy(bytes, 35, primes, 0, primesInFile * 8);

            return new PrimeJob(new PrimeJob.Version(1, 1, 0), batch, start, count, progress, ref primes);
        }
        /// <summary>
        /// Deserializes a <see cref="PrimeJob"/> of version 1.2.0 from a byte array.
        /// </summary>
        /// <param name="bytes">Byte array contaning the serialized <see cref="PrimeJob"/>.</param>
        /// <returns></returns>
        public static PrimeJob Deserializev1_2_0(ref byte[] bytes)
        {
            PrimeJob.Comp comp = new PrimeJob.Comp(bytes[3]);

            uint batch = BitConverter.ToUInt32(bytes, 4);
            ulong start = BitConverter.ToUInt64(bytes, 8);
            ulong count = BitConverter.ToUInt64(bytes, 16);
            ulong progress = BitConverter.ToUInt64(bytes, 24);

            ulong[] primes;
            byte[] primesBytes = new byte[bytes.Length - 32];

            Array.Copy(bytes, 32, primesBytes, 0, bytes.Length - 32);

            if (comp.NCC)
                primes = Compression.NCC.Uncompress(primesBytes);
            else if (comp.ONSS)
                primes = Compression.ONSS.Uncompress(primesBytes);
            else
                primes = GetRawPrimes(primesBytes);

            return new PrimeJob(new PrimeJob.Version(1, 2, 0), batch, start, count, progress, ref primes);
        }



        /// <summary>
        /// Peeks status from a <see cref="PrimeJob"/> of version 1.0.0 from a byte array.
        /// </summary>
        /// <param name="allBytes"><see cref="byte"/> array to peek status from.</param>
        /// <returns></returns>
        public static PrimeJob.Status PeekStatusv1_0_0(ref byte[] allBytes)
        {
            PrimeJob.Status status;

            byte[] bytes = new byte[16];
            Array.Copy(allBytes, 11, bytes, 0, 16);

            ulong count = BitConverter.ToUInt64(bytes, 0);
            ulong progress = BitConverter.ToUInt64(bytes, 8);

            if (progress == 0)
                status = PrimeJob.Status.Not_started;
            else if (progress == count)
                status = PrimeJob.Status.Finished;
            else
                status = PrimeJob.Status.Started;

            return status;
        }
        /// <summary>
        /// Peeks status from a <see cref="PrimeJob"/> of version 1.1.0 from a byte array.
        /// </summary>
        /// <param name="allBytes"><see cref="byte"/> array to peek status from.</param>
        /// <returns></returns>
        public static PrimeJob.Status PeekStatusv1_1_0(ref byte[] allBytes)
        {
            PrimeJob.Status status;

            byte[] bytes = new byte[16];
            Array.Copy(allBytes, 15, bytes, 0, 16);

            ulong count = BitConverter.ToUInt64(bytes, 0);
            ulong progress = BitConverter.ToUInt64(bytes, 8);

            if (progress == 0)
                status = PrimeJob.Status.Not_started;
            else if (progress == count)
                status = PrimeJob.Status.Finished;
            else
                status = PrimeJob.Status.Started;

            return status;
        }
        /// <summary>
        /// Peeks status from a <see cref="PrimeJob"/> of version 1.2.0 from a byte array.
        /// </summary>
        /// <param name="allBytes"><see cref="byte"/> array to peek status from.</param>
        /// <returns></returns>
        public static PrimeJob.Status PeekStatusv1_2_0(ref byte[] allBytes)
        {
            PrimeJob.Status status;

            byte[] bytes = new byte[16];
            Array.Copy(allBytes, 16, bytes, 0, 16);

            ulong count = BitConverter.ToUInt64(bytes, 0);
            ulong progress = BitConverter.ToUInt64(bytes, 8);

            if (progress == 0)
                status = PrimeJob.Status.Not_started;
            else if (progress == count)
                status = PrimeJob.Status.Finished;
            else
                status = PrimeJob.Status.Started;

            return status;
        }



        /// <summary>
        /// Serializes a <see cref="PrimeJob"/> of version 1.0.0 to a byte array.
        /// </summary>
        /// <param name="job"><see cref="PrimeJob"/> to serialize.</param>
        /// <returns></returns>
        public static byte[] Serializev1_0_0(ref PrimeJob job)
        {
            byte[] bytes = new byte[31 + job.Primes.Count * 8];

            bytes[0] = job.FileVersion.major; bytes[1] = job.FileVersion.minor; bytes[2] = job.FileVersion.patch;

            Array.Copy(BitConverter.GetBytes(job.Start), 0, bytes, 3, 8);
            Array.Copy(BitConverter.GetBytes(job.Count), 0, bytes, 11, 8);
            Array.Copy(BitConverter.GetBytes(job.Progress), 0, bytes, 19, 8);
            Array.Copy(BitConverter.GetBytes(job.Primes.Count), 0, bytes, 27, 4);

            Buffer.BlockCopy(job.Primes.ToArray(), 0, bytes, 31, job.Primes.Count * 8);

            return bytes;
        }
        /// <summary>
        /// Serializes a <see cref="PrimeJob"/> of version 1.1.0 to a byte array.
        /// </summary>
        /// <param name="job"><see cref="PrimeJob"/> to serialize.</param>
        /// <returns></returns>
        public static byte[] Serializev1_1_0(ref PrimeJob job)
        {
            byte[] bytes = new byte[35 + job.Primes.Count * 8];

            bytes[0] = job.FileVersion.major; bytes[1] = job.FileVersion.minor; bytes[2] = job.FileVersion.patch;

            Array.Copy(BitConverter.GetBytes(job.Batch), 0, bytes, 3, 4);
            Array.Copy(BitConverter.GetBytes(job.Start), 0, bytes, 7, 8);
            Array.Copy(BitConverter.GetBytes(job.Count), 0, bytes, 15, 8);
            Array.Copy(BitConverter.GetBytes(job.Progress), 0, bytes, 23, 8);
            Array.Copy(BitConverter.GetBytes(job.Primes.Count), 0, bytes, 31, 4);

            Buffer.BlockCopy(job.Primes.ToArray(), 0, bytes, 35, job.Primes.Count * 8);

            return bytes;
        }
        /// <summary>
        /// Serializes a <see cref="PrimeJob"/> of version 1.2.0 to a byte array.
        /// </summary>
        /// <param name="job"><see cref="PrimeJob"/> to serialize.</param>
        /// <returns></returns>
        public static byte[] Serializev1_2_0(ref PrimeJob job)
        {
            List<byte> bytes = new List<byte>(new byte[] { job.FileVersion.major, job.FileVersion.minor, job.FileVersion.patch, job.FileCompression.GetByte() }); //missing size for compressed primes

            bytes.AddRange(BitConverter.GetBytes(job.Batch));
            bytes.AddRange(BitConverter.GetBytes(job.Start));
            bytes.AddRange(BitConverter.GetBytes(job.Count));
            bytes.AddRange(BitConverter.GetBytes(job.Progress));
            bytes.AddRange(BitConverter.GetBytes(job.Primes.Count));

            byte[] primeBytes;

            if (job.FileCompression.IsCompressed())
            {
                if (job.FileCompression.ONSS)
                {
                    primeBytes = Compression.ONSS.Compress(job.Primes.ToArray());
                }
                else if (job.FileCompression.NCC)
                {
                    primeBytes = Compression.NCC.Compress(job.Primes.ToArray());
                }
                else
                    throw new Compression.InvalidCompressionMethodException();
            }
            else
            {
                primeBytes = new byte[job.Primes.Count * 8];

                Buffer.BlockCopy(job.Primes.ToArray(), 0, primeBytes, 0, job.Primes.Count * 8);
            }

            bytes.AddRange(primeBytes);

            return bytes.ToArray();
        }



        private static ulong[] GetRawPrimes(byte[] bytes)
        {
            if (bytes.Length % 8 != 0)
                throw new ArgumentException("Byte array must have a length that is dividable by 8.");

            ulong[] ulongs = new ulong[bytes.Length / 8];

            Buffer.BlockCopy(bytes, 0, ulongs, 0, ulongs.Length);

            return ulongs;
        }
    }



    /// <summary>
    /// Class that contains several <see cref="KnownPrimesResourceFile"/> serialization related methods.
    /// </summary>
    public static class KnownPrimesResourceFileSerializer
    {
        //Version Definitions
        /*  knownPrimes.rsrc v1.0.0
         *  3 bytes     Version     version (1 byte major, 1 byte minor, 1 byte patch)
         *  4 bytes     int         primesInFile
         *  xxx         ulong[]     primes
         */
        /*  knownPrimes.rsrc v1.1.0
         *  3 bytes     Version     version (1 byte major, 1 byte minor, 1 byte patch)
         *  8 bytes     ulong       highestCheckedInFile (highest number till which we checked to make this file, used to later append more primes to the file)
         *  4 bytes     int         primesInFile
         *  xxx         ulong[]     primes
         */



        /// <summary>
        /// Deserializes a <see cref="KnownPrimesResourceFile"/> of version 1.0.0 from a byte array.
        /// </summary>
        /// <param name="bytes">Byte array contaning the serialized <see cref="KnownPrimesResourceFile"/>.</param>
        /// <returns></returns>
        public static KnownPrimesResourceFile Deserializev1_0_0(byte[] bytes)
        {
            int primesInFile = BitConverter.ToInt32(bytes, 3);

            ulong[] primes = new ulong[primesInFile];
            Buffer.BlockCopy(bytes, 7, primes, 0, primesInFile * 8);

            return new KnownPrimesResourceFile(new KnownPrimesResourceFile.Version(1, 0, 0), primes);
        }
        /// <summary>
        /// Deserializes a <see cref="KnownPrimesResourceFile"/> of version 1.1.0 from a byte array.
        /// </summary>
        /// <param name="bytes">Byte array contaning the serialized <see cref="KnownPrimesResourceFile"/>.</param>
        /// <returns></returns>
        public static KnownPrimesResourceFile Deserializev1_1_0(byte[] bytes)
        {
            ulong highestCheckedInFile = BitConverter.ToUInt64(bytes, 3);
            int primesInFile = BitConverter.ToInt32(bytes, 11);

            ulong[] primes = new ulong[primesInFile];
            Buffer.BlockCopy(bytes, 15, primes, 0, primesInFile * 8);

            return new KnownPrimesResourceFile(new KnownPrimesResourceFile.Version(1, 1, 0), highestCheckedInFile, primes);
        }



        /// <summary>
        /// Serializes a <see cref="KnownPrimesResourceFile"/> of version 1.0.0 to a byte array.
        /// </summary>
        /// <param name="file"><see cref="KnownPrimesResourceFile"/> to serialize.</param>
        /// <returns></returns>
        public static byte[] Serializev1_0_0(ref KnownPrimesResourceFile file)
        {
            byte[] bytes = new byte[7 + file.Primes.Length * 8];

            bytes[0] = file.FileVersion.major; bytes[1] = file.FileVersion.minor; bytes[2] = file.FileVersion.patch;

            Array.Copy(BitConverter.GetBytes(file.Primes.Length), 0, bytes, 3, 4);

            Buffer.BlockCopy(file.Primes, 0, bytes, 7, file.Primes.Length * 8);

            return bytes;
        }
        /// <summary>
        /// Serializes a <see cref="KnownPrimesResourceFile"/> of version 1.1.0 to a byte array.
        /// </summary>
        /// <param name="file"><see cref="KnownPrimesResourceFile"/> to serialize.</param>
        /// <returns></returns>
        public static byte[] Serializev1_1_0(ref KnownPrimesResourceFile file)
        {
            byte[] bytes = new byte[15 + file.Primes.Length * 8];

            bytes[0] = file.FileVersion.major; bytes[1] = file.FileVersion.minor; bytes[2] = file.FileVersion.patch;

            Array.Copy(BitConverter.GetBytes(file.HighestCheckedInFile), 0, bytes, 3, 8);
            Array.Copy(BitConverter.GetBytes(file.Primes.Length), 0, bytes, 11, 4);

            Buffer.BlockCopy(file.Primes, 0, bytes, 15, file.Primes.Length * 8);

            return bytes;
        }
    }
}