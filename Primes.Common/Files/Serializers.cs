using System;
using System.Collections.Generic;
using System.IO;

using Primes.Common.ErrorCorrection;

namespace Primes.Common.Files
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
        /* v1.3.0 (WIP)
         *  0   3 bytes     Version     version (1 byte major, 1 byte minor, 1 byte patch)
         *  3   1 byte      Comp        compression header (0b000000, 1 bit NNC, 1 bit ONSS)
         *  4   4 bytes     uint        batch
         *  8   8 bytes     ulong       start
         *  16  8 bytes     ulong       count
         *  24  8 bytes     ulong       progress
         *  32  2 bytes     ushort      header checksum (fletcher 16)
         *  34  xxx         EPB         error protected blocks containing primes (optionally compressed) (fletcher 16 in blocks of 4096 bytes) (smaller blocks so little is wasted when corrupted blocks)
         */



        /// <summary>
        /// Deserializes a <see cref="PrimeJob"/> of version 1.0.0 from a byte array.
        /// </summary>
        /// <param name="bytes">Byte array contaning the serialized <see cref="PrimeJob"/>.</param>
        /// <returns></returns>
        public static PrimeJob Deserializev1_0_0(byte[] bytes)
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
        public static PrimeJob Deserializev1_1_0(byte[] bytes)
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
        public static PrimeJob Deserializev1_2_0(byte[] bytes)
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

            return new PrimeJob(new PrimeJob.Version(1, 2, 0), comp, batch, start, count, progress, ref primes);
        }
        /// <summary>
        /// Deserializes a <see cref="PrimeJob"/> of version 1.3.0 from a byte array.
        /// </summary>
        /// <param name="bytes">Byte array contaning the serialized <see cref="PrimeJob"/>.</param>
        /// <returns></returns>
        [Obsolete("Not fully implemented yet.")]
        public static PrimeJob Deserializev1_3_0(byte[] bytes)
        {
            throw new NotImplementedException();

            PrimeJob.Comp comp = new PrimeJob.Comp(bytes[3]);

            uint batch = BitConverter.ToUInt32(bytes, 4);
            ulong start = BitConverter.ToUInt64(bytes, 8);
            ulong count = BitConverter.ToUInt64(bytes, 16);
            ulong progress = BitConverter.ToUInt64(bytes, 24);
            ushort headerChecksum = BitConverter.ToUInt16(bytes, 32);

            byte[] headerBytes = new byte[32];
            Array.Copy(bytes, 0, headerBytes, 0, 32);

            if (Fletcher.Fletcher16(headerBytes) != headerChecksum)
                throw new Exception("Corrupted header.");

            byte[] block;
            List<byte> primeBytes = new List<byte>();
            int header = 34;
            
            while (true)
            {
                //how you keepin track of what's been used and what's not
                //(streams ofc)

                break;
            }

            throw new NotImplementedException();
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
        public static byte[] Serializev1_0_0(PrimeJob job)
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
        public static byte[] Serializev1_1_0(PrimeJob job)
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
        public static byte[] Serializev1_2_0(PrimeJob job)
        {
            List<byte> bytes = new List<byte>(new byte[] { job.FileVersion.major, job.FileVersion.minor, job.FileVersion.patch, job.FileCompression.GetByte() });

            bytes.AddRange(BitConverter.GetBytes(job.Batch));
            bytes.AddRange(BitConverter.GetBytes(job.Start));
            bytes.AddRange(BitConverter.GetBytes(job.Count));
            bytes.AddRange(BitConverter.GetBytes(job.Progress));

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
        /// <summary>
        /// Serializes a <see cref="PrimeJob"/> of version 1.3.0 to a byte array.
        /// </summary>
        /// <param name="job"><see cref="PrimeJob"/> to serialize.</param>
        /// <returns></returns>
        [Obsolete("Not fully implemented yet.")]
        public static byte[] Serializev1_3_0(PrimeJob job)
        {
            throw new NotImplementedException();

            const int EPB_DataSize = 4096;

            List<byte> bytes = new List<byte>(new byte[] { job.FileVersion.major, job.FileVersion.minor, job.FileVersion.patch, job.FileCompression.GetByte() });

            bytes.AddRange(BitConverter.GetBytes(job.Batch));
            bytes.AddRange(BitConverter.GetBytes(job.Start));
            bytes.AddRange(BitConverter.GetBytes(job.Count));
            bytes.AddRange(BitConverter.GetBytes(job.Progress));
            bytes.AddRange(BitConverter.GetBytes(Fletcher.Fletcher16(bytes.ToArray())));

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

            ErrorProtectedBlock[] blocks = new ErrorProtectedBlock[Mathf.DivideRoundUp(primeBytes.Length, EPB_DataSize)];

            byte[] buffer = new byte[EPB_DataSize];

            for (int i = 0; i < blocks.Length; i++)
            {
                int dataInBlock = Mathf.Clamp(primeBytes.Length - (i * (EPB_DataSize)), 1, EPB_DataSize);
                Array.Copy(primeBytes, i * (EPB_DataSize), buffer, 0, dataInBlock);
                blocks[i] = new ErrorProtectedBlock(ErrorProtectedBlock.ErrorProtectionType.Fletcher16, buffer, BitConverter.GetBytes(Fletcher.Fletcher16(buffer)));
            }

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
         *  0   3 bytes     Version     version (1 byte major, 1 byte minor, 1 byte patch)
         *  3   4 bytes     int         primesInFile
         *  7   xxx         ulong[]     primes
         */
        /*  knownPrimes.rsrc v1.1.0
         *  0   3 bytes     Version     version (1 byte major, 1 byte minor, 1 byte patch)
         *  3   8 bytes     ulong       highestCheckedInFile (highest number till which we checked to make this file, used to later append more primes to the file)
         *  11  4 bytes     int         primesInFile
         *  15  xxx         ulong[]     primes
         */
        /* knownPrimes.rsrc v1.2.0
         *  0   3 bytes     Version     version (1 byte major, 1 byte minor, 1 byte patch)
         *  3   1 byte      Comp        compression header (0b000000, 1 bit NNC, 1 bit ONSS)
         *  4   4 bytes     int         primeCount
         *  8   xxx         ulong[]     (compressed) primes
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
        /// Deserializes a <see cref="KnownPrimesResourceFile"/> of version 1.2.0 from a byte array.
        /// </summary>
        /// <param name="bytes">Byte array contaning the serialized <see cref="KnownPrimesResourceFile"/>.</param>
        /// <returns></returns>
        public static KnownPrimesResourceFile Deserializev1_2_0(byte[] bytes)
        {
            KnownPrimesResourceFile.Comp comp = new KnownPrimesResourceFile.Comp(bytes[4]);

            byte[] primeBytes = new byte[bytes.Length - 4];
            KnownPrimesResourceFile file = new KnownPrimesResourceFile(new KnownPrimesResourceFile.Version(1, 2, 0), comp, new ulong[BitConverter.ToInt32(bytes, 4)]);

            Array.Copy(bytes, 8, primeBytes, 0, primeBytes.Length);

            if (comp.NCC)
                file.Primes = Compression.NCC.Uncompress(primeBytes);
            else if (comp.ONSS)
                file.Primes = Compression.ONSS.Uncompress(primeBytes);
            else
                file.Primes = GetRawPrimes(primeBytes);

            return file;
        }
        /// <summary>
        /// Deserializes a <see cref="KnownPrimesResourceFile"/> of version 1.2.0 from a stream.
        /// </summary>
        /// <param name="stream">Stream containing the <see cref="KnownPrimesResourceFile"/>.</param>
        /// <returns></returns>
        /// <remarks>Useful for handling large files.</remarks>
        public static KnownPrimesResourceFile Deserializev1_2_0(Stream stream)
        {
            byte[] buffer = new byte[5];

            stream.Seek(3, SeekOrigin.Begin);
            stream.Read(buffer, 0, 5);

            KnownPrimesResourceFile.Comp comp = new KnownPrimesResourceFile.Comp(buffer[0]);
            KnownPrimesResourceFile file = new KnownPrimesResourceFile(new KnownPrimesResourceFile.Version(1, 2, 0), comp, new ulong[BitConverter.ToInt32(buffer, 1)]);

            if (comp.NCC)
                Compression.NCC.StreamUncompress(stream, file.Primes);
            else if (comp.ONSS)
            {
                byte[] primeBytes = new byte[stream.Length - 5];
                stream.Read(primeBytes, 0, primeBytes.Length);

                file.Primes = Compression.ONSS.Uncompress(primeBytes);
            }
            else
            {
                byte[] primeBytes = new byte[stream.Length - 5];
                stream.Read(primeBytes, 0, primeBytes.Length);

                file.Primes = GetRawPrimes(primeBytes);
            }

            return file;
        }



        /// <summary>
        /// Serializes a <see cref="KnownPrimesResourceFile"/> of version 1.0.0 to a byte array.
        /// </summary>
        /// <param name="file"><see cref="KnownPrimesResourceFile"/> to serialize.</param>
        /// <returns></returns>
        public static byte[] Serializev1_0_0(KnownPrimesResourceFile file)
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
        public static byte[] Serializev1_1_0(KnownPrimesResourceFile file)
        {
            byte[] bytes = new byte[15 + file.Primes.Length * 8];

            bytes[0] = file.FileVersion.major; bytes[1] = file.FileVersion.minor; bytes[2] = file.FileVersion.patch;

            Array.Copy(BitConverter.GetBytes(file.HighestCheckedInFile), 0, bytes, 3, 8);
            Array.Copy(BitConverter.GetBytes(file.Primes.Length), 0, bytes, 11, 4);

            Buffer.BlockCopy(file.Primes, 0, bytes, 15, file.Primes.Length * 8);

            return bytes;
        }
        /// <summary>
        /// Serializes a <see cref="KnownPrimesResourceFile"/> of version 1.2.0 to a byte array.
        /// </summary>
        /// <param name="file"><see cref="KnownPrimesResourceFile"/> to serialize.</param>
        /// <returns></returns>
        public static byte[] Serializev1_2_0(KnownPrimesResourceFile file)
        {
            MemoryStream stream = new MemoryStream();

            stream.Write(new byte[] { file.FileVersion.major, file.FileVersion.minor, file.FileVersion.patch, file.FileCompression.GetByte() }, 0, 4);
            stream.Write(BitConverter.GetBytes(file.Primes.Length), 0, 4);

            ulong last = 0;
            Compression.NCC.StreamCompress(stream, file.Primes, ref last);

            return stream.ToArray();
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
}