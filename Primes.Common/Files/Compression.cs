using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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



            /*public static byte[] CompressDifferences(ulong[] ulongs)
            {
                ushort[] diffs = new ushort[];

                throw new NotImplementedException();
                return bytes;
            }*/
            public static byte[] CompressAbsolutes(ulong[] ulongs)
            {
                byte[] bytes = new byte[ulongs.Length * 8];
                Buffer.BlockCopy(ulongs, 0, bytes, 0, bytes.Length);

                List<Node<byte>> nodes = new List<Node<byte>>(256);
                float[] frequencies = new float[256];

                for (int i = 0; i < bytes.Length; i++)
                    frequencies[bytes[i]]++;

                for (int i = 0; i < frequencies.Length; i++)
                    nodes.Add(new Node<byte>((byte)i, frequencies[i] / bytes.Length));

                nodes.RemoveAll((x) => x.Frequency == 0);

                foreach (var n in nodes)
                    Console.WriteLine($"symbol {n.Symbol} freq {(n.Frequency * 100f):F2}");

                Node<byte> treeRoot = BuildNodeTreeByte(nodes, out Dictionary<byte, List<bool>> dictionary);

                Console.WriteLine(GetTreeString(treeRoot));

                List<bool> outBits = new List<bool>();
                
                for (int i = 0; i < bytes.Length; i++)
                    outBits.AddRange(dictionary[bytes[i]].ToArray());

                int[] ints = outBits.ToIntArray(out int usedBits);
                byte[] dictionaryBytes = SerializeByteDictionary(dictionary);

                int len = 4 + dictionaryBytes.Length + Mathf.DivideRoundUp(usedBits, 8);
                int intBytes = Mathf.DivideRoundUp(usedBits, 8);
                bytes = new byte[4 + dictionaryBytes.Length + Mathf.DivideRoundUp(usedBits, 8)]; //0 = int primesInFile
                Array.Copy(BitConverter.GetBytes(ulongs.Length), 0, bytes, 0, 4);
                Array.Copy(dictionaryBytes, 0, bytes, 4, dictionaryBytes.Length);
                Buffer.BlockCopy(ints, 0, bytes, 4 + dictionaryBytes.Length, intBytes);

                Console.WriteLine(GetDictionaryString(dictionary));

                return bytes;
            }
            public static ulong[] UncompressAbsolutes(byte[] bytes)
            {
                int primesInFile = BitConverter.ToInt32(bytes, 0);
                ulong[] ulongs = new ulong[primesInFile];

                Dictionary<byte, List<bool>> dictionary = DeserializeByteDictionary(bytes, 4, out int byteHeader);
                Console.WriteLine(GetDictionaryString(dictionary));
                int bitHeader = 0;
                byte[] tmpBytes = new byte[8];

                for (int i = 0; i < ulongs.Length; i++)
                {
                    for (int b = 0; b < 8; b++)
                    {
                        tmpBytes[b] = ExtractValue(bytes, byteHeader, bitHeader, dictionary, out byteHeader, out bitHeader);
                    }

                    ulongs[i] = BitConverter.ToUInt64(tmpBytes, 0);
                }

                return ulongs;
            }



            private static Node<byte> BuildNodeTreeByte(List<Node<byte>> nodes, out Dictionary<byte, List<bool>> dictionary)
            {
                Console.WriteLine("==Build Node Tree Byte==");

                dictionary = new Dictionary<byte, List<bool>>();

                Node<byte> newNode = null;

                while (nodes.Count > 1)
                {
                    nodes.Sort((x, y) => x.Frequency.CompareTo(y.Frequency));
                    Node<byte> n1 = nodes[0], n2 = nodes[1];

                    newNode = new Node<byte>(0, n1.Frequency + n2.Frequency, n1, n2);
                    n1.Parent = newNode;
                    n2.Parent = newNode;

                    nodes.Remove(n1);
                    nodes.Remove(n2);
                    nodes.Add(newNode);

                    if (n1.child1 == null)
                        dictionary.Add(n1.Symbol, new List<bool>(new bool[] { false }));
                    else
                        RecursiveAppendBool(n1, false, dictionary);

                    if (n2.child1 == null)
                        dictionary.Add(n2.Symbol, new List<bool>(new bool[] { true }));
                    else
                        RecursiveAppendBool(n2, true, dictionary);

                    Console.WriteLine($"iters for {n1.Symbol} and {n2.Symbol}");
                }

                return newNode;
            }
            private static string GetTreeString<T>(Node<T> root)
            {
                Console.WriteLine("==Get Tree String==");

                string ret = string.Empty;

                if (root.child1 != null)
                    ret += $"{{{GetTreeString(root.child1)}, {GetTreeString(root.child2)}}}";
                else
                    ret += $"={root.Symbol}=";

                return ret;
            }
            private static byte[] SerializeByteDictionary(Dictionary<byte, List<bool>> dictionary)
            {
                Console.WriteLine("==Serialize Byte Dictionary==");

                byte[] keys = dictionary.GetKeys();
                List<bool>[] values = dictionary.GetValues();
                List<byte> bytes = new List<byte>();

                bytes.Add((byte)dictionary.Count);

                for (int i = 0; i < keys.Length; i++)
                {
                    int[] ints = values[i].ToIntArray(out int bitsUsed);

                    byte[] localBytes = new byte[Mathf.DivideRoundUp(bitsUsed, 8)];
                    Buffer.BlockCopy(ints, 0, localBytes, 0, localBytes.Length);//the rest is padded with 0s

                    bytes.Add(keys[i]);
                    bytes.Add((byte)bitsUsed);
                    bytes.AddRange(localBytes);

                    Console.WriteLine($"len {localBytes.Length} first {localBytes[0]}");
                    foreach (bool b in values[i])
                        Console.Write(b.ToString() + " ");

                    Console.Write('\n');
                }

                return bytes.ToArray();
            }
            private static void RecursiveAppendBool(Node<byte> node, bool value, Dictionary<byte, List<bool>> dictionary)
            {
                if (node.child1 != null)
                {
                    RecursiveAppendBool(node.child1, value, dictionary);
                    RecursiveAppendBool(node.child2, value, dictionary);
                }
                else
                    dictionary[node.Symbol].Add(value);
            }
            private static Dictionary<byte, List<bool>> DeserializeByteDictionary(byte[] bytes, int startAddress, out int nextAddress)
            {
                byte entryCount = bytes[startAddress];

                Dictionary<byte, List<bool>> ret = new Dictionary<byte, List<bool>>(entryCount);

                int header = startAddress + 1;

                for (int i = 0; i < entryCount; i++)
                {
                    byte symbol = bytes[header++];
                    byte bitsUsed = bytes[header++];

                    byte[] localBytes = new byte[Mathf.DivideRoundUp(bitsUsed, 8)];
                    Array.Copy(bytes, header, localBytes, 0, localBytes.Length);
                    header += localBytes.Length;

                    List<bool> value = new List<bool>(BoolArrFromBytes(localBytes, bitsUsed));

                    //Console.WriteLine($"Symbol {symbol} value below local.Len {localBytes.Length} value f{localBytes[0]}");
                    //foreach (bool b in value)
                        //Console.Write(b.ToString() + " ");
                    //Console.Write('\n');

                    ret.Add(symbol, value);
                }

                nextAddress = header;
                return ret;
            }
            private static bool[] BoolArrFromBytes(byte[] bytes, byte bits)
            {
                bool[] ret = new bool[bits];

                for (int i = 0, b = 0; i < bits; i++)
                {
                    ret[i] = ((bytes[b] >> (i % 8)) % 2) == 0;

                    Console.WriteLine($"i{i} b{b} byte{bytes[b]:B} ret{ret[i]}");

                    blah
                        //broken af

                    if ((i % 8) == 7)
                        b++;
                }
                Console.WriteLine();

                return ret;
            }
            private static byte ExtractValue(byte[] bytes, int startIndex, int startBit, Dictionary<byte, List<bool>> dictionary, out int endIndex, out int endBit)
            {
                Console.WriteLine("==Extract Value==");

                byte[] symbols = dictionary.GetKeys();
                List<bool>[] values = dictionary.GetValues();

                int byteHeader = startIndex;
                int bitHeader = startBit;
                List<bool> currentVal = new List<bool>();

                while(true)
                {
                    bool step = (bytes[byteHeader] << bitHeader++) == 0;

                    if (bitHeader >= 8)
                    {
                        byteHeader++;
                        bitHeader = 0;
                    }

                    currentVal.Add(step);

                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] == currentVal)
                        {
                            endIndex = byteHeader;
                            endBit = bitHeader;
                            return symbols[i];
                        }
                    }
                }

                throw new Exception("Invalid value!");
            }
            private static string GetDictionaryString(Dictionary<byte, List<bool>> dictionary)
            {
                Console.WriteLine("==Get Dictionary String==");

                string ret = string.Empty;
                foreach (var pair in dictionary)
                    ret += $"{pair.Key}:{pair.Value.ToIntArray(out int _)[0]}\n";

                return ret;
            }


            
            private class Node<TSymbol>
            {
                public TSymbol Symbol;
                public float Frequency;
                public Node<TSymbol> Parent;
                public Node<TSymbol> child1, child2;

                public Node(TSymbol symbol, float frequency) { Symbol = symbol; Frequency = frequency; Parent = null; child1 = null; child2 = null; }
                public Node(TSymbol symbol, float frequency, Node<TSymbol> parent) { Symbol = symbol; Frequency = frequency; Parent = parent; child1 = null; child2 = null; }
                public Node(TSymbol symbol, float frequency, Node<TSymbol> child1, Node<TSymbol> child2) { Symbol = symbol; Frequency = frequency; Parent = null; this.child1 = child1; this.child2 = child2; }
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
