using System;
using System.Collections.Generic;
using System.IO;

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
                    }
                    else
                    {
                        bytes.AddRange(BitConverter.GetBytes((ushort)delta));
                    }

                    header++;
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
            private const int BlockSize = 65536;



            /// <summary>
            /// Compresses an <see cref="ulong"/> array using NCC.
            /// </summary>
            /// <param name="ulongs">The ulong array to compress</param>
            /// <returns>Byte array with the compressed ulongs</returns>
            public static byte[] Compress(ulong[] ulongs)
            {
                List<byte> bytes = new List<byte>();

                //first is always raw

                if (ulongs.Length == 0)
                    goto end;

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
                    }
                    else
                    {
                        bytes.AddRange(BitConverter.GetBytes((ushort)delta));
                    }

                    header++;
                }

                end:
                return bytes.ToArray();
            }
            /// <summary>
            /// Uncompresses an <see cref="ulong"/> array using NCCS.
            /// </summary>
            /// <param name="bytes">The byte array to uncompress.</param>
            /// <returns>Ulong array with the uncompressed values.</returns>
            public static ulong[] Uncompress(byte[] bytes)
            {
                if (bytes.Length == 0)
                    return new ulong[0];

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



            /// <summary>
            /// Writes an array of compressed ulongs to a stream.
            /// </summary>
            /// <param name="stream">The stream to write to.</param>
            /// <param name="append">The array to compress.</param>
            /// <param name="last">The last value compressed to the stream. Will update once compression is complete.</param>
            /// <remarks>Useful when dealing with large data sets.</remarks>
            public static void StreamCompress(Stream stream, ulong[] append, ref ulong last)
            {
                int header = 0;
                ulong delta;
                List<byte> block = new List<byte>();

                if (last == 0)
                {
                    block.AddRange(BitConverter.GetBytes(append[header++]));
                    last = append[0];
                }

                while (header < append.Length)
                {
                    delta = append[header] - last;

                    if (delta > (ulong)0xFFFF)
                    {
                        block.AddRange(new byte[] { 0, 0 });
                        block.AddRange(BitConverter.GetBytes(append[header]));
                    }
                    else
                    {
                        block.AddRange(BitConverter.GetBytes((ushort)delta));
                    }

                    last = append[header];
                    header++;

                    if (block.Count > BlockSize)
                    {
                        stream.Write(block.ToArray(), 0, block.Count);
                        stream.Flush();
                        block.Clear();
                    }
                }

                stream.Write(block.ToArray(), 0, block.Count);
                block.Clear();
                stream.Flush();
            }
            /// <summary>
            /// Reads an array of compressed ulongs from a stream.
            /// </summary>
            /// <param name="stream">The stream to read from.</param>
            /// <param name="uncompress">The uncompressed ulong array. (Must be set before calling).</param>
            /// <remarks>Useful when dealing with large data sets.</remarks>
            public static void StreamUncompress(Stream stream, ulong[] uncompress)
            {
                if (stream.RemainingBytes() == 0)
                    return;

                ushort delta;
                byte[] block = new byte[BlockSize];
                ulong value, last = 0;
                int unHeader = 0, blockHeader;
                bool pendingBlockRead = true;


            loadBlock:
                LoadBlock(ref stream, ref block, ref pendingBlockRead, 0);

                blockHeader = 0;

                if (last == 0)
                {
                    uncompress[unHeader] = BitConverter.ToUInt64(block, blockHeader);
                    blockHeader += 8;
                    last = uncompress[unHeader++];
                }

                while (blockHeader < block.Length - 1)
                {
                    delta = BitConverter.ToUInt16(block, blockHeader);
                    blockHeader += 2;

                    if (delta == 0)
                    {
                        if (blockHeader >= block.Length)
                        {
                            if (!pendingBlockRead)
                                throw new Exception("Out of blocks.");

                            LoadBlock(ref stream, ref block, ref pendingBlockRead, block.Length - blockHeader);
                            blockHeader -= BlockSize;
                        }

                        value = BitConverter.ToUInt64(block, blockHeader);
                        blockHeader += 8;

                        uncompress[unHeader++] = value;
                        last = value;
                    }
                    else
                    {
                        value = last + delta;

                        uncompress[unHeader++] = value;
                        last = value;
                    }
                }

                if (pendingBlockRead)
                    goto loadBlock;
            }



            private static void LoadBlock(ref Stream stream, ref byte[] block, ref bool pendingBlockRead, int preserve)
            {
                int size = preserve + (int)Math.Min(BlockSize, stream.RemainingBytes());

                block = new byte[size];
                stream.Read(block, preserve, (int)Math.Min(BlockSize, stream.RemainingBytes()));

                if (stream.RemainingBytes() == 0)
                    pendingBlockRead = false;
            }
        }



        public static class HuffmanCoding
        {
            private const int BlockSize = 65536;



            public static byte[] CompressDifferences(ulong[] ulongs)
            {
                ushort[] diffs = new ushort[];

                throw new NotImplementedException();
                return bytes;
            }
            public static byte[] CompressAbsolutes(ulong[] ulongs)
            {
                byte[] bytes = new byte[ulongs.Length * 8];
                Buffer.BlockCopy(ulongs, 0, bytes, 0, bytes.Length);

                float[] frequencies = new float[256];

                for (int i = 0; i < bytes.Length; i++)
                    frequencies[bytes[i]]++;

                Node<byte>[] nodes = new Node<byte>[256];

                throw new NotImplementedException();
                return bytes;
            }


            
            private class Node<TSymbol>
            {
                public TSymbol Symbol;
                public float Frequency;
                public Node<TSymbol> Parent;
                public Node<TSymbol> child1, child2;

                public Node(TSymbol symbol, float frequency) { Symbol = symbol; Frequency = frequency; Parent = null; child1 = null; child2 = null; }
                public Node(TSymbol symbol, float frequency, Node<TSymbol> parent) { Symbol = symbol; Frequency = frequency; Parent = parent; child1 = null; child2 = null; }
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
