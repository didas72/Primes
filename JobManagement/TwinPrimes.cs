using System;

namespace JobManagement
{
    public class TwinPrimes
    {
        public ulong first;
        public ulong second;



        public TwinPrimes(ulong first) { this.first = first; second = first + 2; }
        public TwinPrimes(ulong first, ulong second) { this.first = first; this.second = second; }



        public static byte[] Serialize(TwinPrimes twins)
        {
            return BitConverter.GetBytes(twins.first);
        }
        public static TwinPrimes Deserialize(byte[] bytes)
        {
            if (bytes.Length != 8) throw new ArgumentException("Byte array must be 8 bytes long.");

            return new TwinPrimes(BitConverter.ToUInt64(bytes, 0));
        }
    }
}
