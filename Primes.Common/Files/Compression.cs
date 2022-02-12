using System;
using System.Collections.Generic;
using System.IO;

using DidasUtils.Extensions;

namespace Primes.Common.Files
{
    /// <summary>
    /// Class that contains several compression related methods.
    /// </summary>
    public static class Compression
    {
        private const int BlockSize = 65536;

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
                    throw new ArgumentException(nameof(ulongs));

                List<byte> bytes = new();

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
                    throw new ArgumentException(nameof(bytes));

                List<ulong> ulongs = new();

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
                List<byte> bytes = new();

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
                    return Array.Empty<ulong>();

                List<ulong> ulongs = new();

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
                List<byte> block = new();

                if (last == 0)
                {
                    block.AddRange(BitConverter.GetBytes(append[header++]));
                    last = append[0];
                }

                while (header < append.Length)
                {
                    delta = append[header] - last;

                    if (delta > (ulong)ushort.MaxValue)
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
            /// <summary>
            /// Reads an list of compressed ulongs from a stream.
            /// </summary>
            /// <param name="stream">The stream to read from.</param>
            /// <param name="uncompress">The uncompressed ulong list.</param>
            /// <remarks>Useful when dealing with large data sets.</remarks>
            public static void StreamUncompress(Stream stream, List<ulong> uncompress)
            {
                if (stream.RemainingBytes() == 0)
                    return;

                ushort delta;
                byte[] block = new byte[BlockSize];
                ulong value, last = 0;
                int blockHeader;
                bool pendingBlockRead = true;


            loadBlock:
                LoadBlock(ref stream, ref block, ref pendingBlockRead, 0);

                blockHeader = 0;

                if (last == 0)
                {
                    uncompress.Add(BitConverter.ToUInt64(block, blockHeader));
                    blockHeader += 8;
                    last = uncompress[^1];
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

                        uncompress.Add(value);
                        last = value;
                    }
                    else
                    {
                        value = last + delta;

                        uncompress.Add(value);
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



        /*public static class HuffmanCoding
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
                //allocate needed memory
                MyBitArray outBits = new MyBitArray();
                List<Node<byte>> nodes = new List<Node<byte>>(256);
                float[] frequencies = new float[256];



                //get the bytes from the absolute values
                byte[] bytes = new byte[ulongs.Length * 8];
                Buffer.BlockCopy(ulongs, 0, bytes, 0, bytes.Length);



                //calculate symbol frequencies
                for (int i = 0; i < bytes.Length; i++)
                    frequencies[bytes[i]]++;



                //create nodes for each symbol
                for (int i = 0; i < frequencies.Length; i++)
                    nodes.Add(new Node<byte>((byte)i, frequencies[i] / bytes.Length));



                //remove empty nodes
                nodes.RemoveAll((x) => x.Frequency == 0);



                //build and print tree and dictionary
                Node<byte> treeRoot = BuildNodeTreeByte(nodes, out Dictionary<byte, List<bool>> dictionary);
                Console.WriteLine(GetTreeString(treeRoot));
                Console.WriteLine(GetDictionaryString(dictionary));



                //add dictionary to the output
                for (int i = 0; i < bytes.Length; i++)
                    outBits.AppendBoolArray(dictionary[bytes[i]].ToArray());

                

                //serialize dictionary and coded data
                byte[] dictionaryBytes = SerializeByteDictionary(dictionary);
                byte[] codedBytes = outBits.Serialize();



                //merge into one array
                int len = dictionaryBytes.Length + codedBytes.Length;
                bytes = new byte[len];
                Array.Copy(dictionaryBytes, 0, bytes, 0, dictionaryBytes.Length);
                Buffer.BlockCopy(codedBytes, 0, bytes, dictionaryBytes.Length, codedBytes.Length);
                


                return bytes;
            }
            public static ulong[] UncompressAbsolutes(byte[] bytes)
            {
                //allocate needed memory
                List<ulong> ret = new List<ulong>();



                //deserialize dictionary and print outputs
                Dictionary<byte, List<bool>> dictionary = DeserializeByteDictionary(bytes, 5, out int byteHeader);
                Console.WriteLine($"Out byteHeader={byteHeader}");
                Console.WriteLine(GetDictionaryString(dictionary));



                //deserialize MyBitArray
                byte[] arrayBytes = new byte[bytes.Length - byteHeader];
                Array.Copy(bytes, byteHeader, arrayBytes, 0, arrayBytes.Length);
                MyBitArray inBits = MyBitArray.Deserialize(arrayBytes);



                int header = 0;
                List<bool> value = new List<bool>();



                while(true)
                {
                    bool step = inBits.GetBool(header++);

                    value.Add(step);

                    foreach (var pair in dictionary)
                    {
                        if (pair.Value.Count != value.Count)
                            continue;

                        bool match = true;

                        for (int i = 0; i < value.Count; i++)
                        {
                            if (pair.Value[i] != value[i])
                            {
                                match = false;
                                break;
                            }
                        }

                        if (!match)
                            continue;

                        ret.Add(pair.Key);
                    }
                }

                

                return ret.ToArray();
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
                //Console.WriteLine("==Get Tree String==");

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
                //Console.WriteLine($"Pre header={header - 1}");

                for (int i = 0; i < entryCount; i++)
                {
                    byte symbol = bytes[header++];
                    byte bitsUsed = bytes[header++];

                    byte[] localBytes = new byte[Mathf.DivideRoundUp(bitsUsed, 8)];
                    Array.Copy(bytes, header, localBytes, 0, localBytes.Length);
                    header += localBytes.Length;

                    List<bool> value = new List<bool>(BoolArrFromBytes(localBytes, bitsUsed));

                    ret.Add(symbol, value);
                }

                //Console.WriteLine($"Deserialized Dictionary entries={entryCount} header={header} dictionarySize={ret.Count}");

                nextAddress = header;
                return ret;
            }
            private static bool[] BoolArrFromBytes(byte[] bytes, byte bits)
            {
                bool[] ret = new bool[bits];

                for (int i = 0; i < bits; i++)
                {
                    //last bool is high bit
                    int shift = i % 7; //first () is absolute shift second is relative shift
                    int byteIndex = shift / 8;

                    ret[i] = ((bytes[byteIndex] >> shift) % 2) != 0;

                    //Console.WriteLine($"i{i} s{shift} b{byteIndex} byte{bytes[byteIndex]} ret{ret[i]}");
                }

                return ret;
            }
            private static byte ExtractValue(byte[] bytes, ref int byteHeader, ref int bitHeader, Dictionary<byte, List<bool>> dictionary)
            {
                Console.WriteLine("==Extract Value==");

                byte[] symbols = dictionary.GetKeys();
                List<bool>[] values = dictionary.GetValues();
                List<bool> currentVal = new List<bool>();

                int counter = 0;
                while(true)
                {
                    Console.WriteLine($"counter{counter} b{byteHeader} i{bitHeader}");

                    //last bool is high bit
                    bool step = (bytes[byteHeader] >> bitHeader++) != 0;

                    if (bitHeader >= 8)
                    {
                        byteHeader++;
                        bitHeader = 0;
                    }

                    currentVal.Add(step);

                    Console.WriteLine("Val");
                    foreach (bool b in currentVal)
                        Console.Write(b);
                    Console.Write('\n');

                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] == currentVal)
                        {
                            return symbols[i];
                        }
                    }

                    counter++;
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
        }*/



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
