using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Primes.Common.Net
{
    public static class SegmentedData
    {
        private const int headerSize = 8;



        public static void SendToStream(byte[] data, NetworkStream stream, int blockSize)
        {
            if (data == null)
                throw new ArgumentException("Data must not be null.");
            if (data.Length <= 0)
                throw new ArgumentException("Data must not be empty.");
            if (stream == null)
                throw new ArgumentException("Stream must not be null.");
            if (!stream.CanWrite)
                throw new ArgumentException("Stream must be writeable.");
            if (blockSize <= 256)
                throw new ArgumentException("Block size must be at least 256 bytes.");

            int dataBlockSize = blockSize - headerSize;
            int remainingBlocks = Mathf.DivideRoundUp(data.Length, dataBlockSize);
            int head = 0;

            while (remainingBlocks > 0)
            {
                int dataInBlock = Math.Min(data.Length - head, dataBlockSize);

                //add header containing: block size, remaining blocks and size of data block
                stream.Write(BitConverter.GetBytes(remainingBlocks), 0, 4);
                stream.Write(BitConverter.GetBytes(dataInBlock), 0, 4);
                stream.Write(data, head, dataInBlock);

                if (remainingBlocks == 0)
                {
                    byte[] padding = new byte[dataBlockSize - dataInBlock];
                    Array.Clear(padding, 0, padding.Length);

                    stream.Write(padding, 0, padding.Length);
                }

                head += dataInBlock;

                stream.Flush();

                remainingBlocks--;
            }
        }

        public static byte[] ReadFromStream(NetworkStream stream, int blockSize)
        {
            if (!stream.CanRead)
                throw new ArgumentException("Stream must be readable.");
            if (blockSize <= 256)
                throw new ArgumentException("Block size must be at least 256 bytes.");

            List<byte> bytes = new List<byte>();

            while (true)
            {
                byte[] block = new byte[blockSize];

                stream.Read(block, 0, blockSize);

                int remainingBlocks = BitConverter.ToInt32(block, 0) - 1;
                int dataInBlock = BitConverter.ToInt32(block, 4);

                byte[] dataBytes = new byte[dataInBlock];
                Array.Copy(block, headerSize, dataBytes, 0, dataInBlock);

                bytes.AddRange(dataBytes);

                if (remainingBlocks == 0)
                    break;
            }

            return bytes.ToArray();
        }
    }
}
