using System;

namespace JobManagement
{
    public class PrimeDensity
    {
        public ulong start;
        public ulong length;
        public ulong count;
        public double Density { get { return (double)count / (double)length; } }



        public const int size = 3 * sizeof(ulong);



        private PrimeDensity() { }
        public PrimeDensity(ulong start, ulong length) { this.start = start; this.length = length; count = 0; }
        public PrimeDensity(ulong start, ulong length, ulong count) { this.start = start; this.length = length; this.count = count; }



        public static byte[] Serialize(PrimeDensity density)
        {
            byte[] ret = new byte[size];

            Array.Copy(BitConverter.GetBytes(density.start), 0, ret, 0, 8);
            Array.Copy(BitConverter.GetBytes(density.length), 0, ret, 8, 8);
            Array.Copy(BitConverter.GetBytes(density.count), 0, ret, 16, 8);

            return ret;
        }

        public static PrimeDensity Deserialize(byte[] bytes)
        {
            if (bytes.Length != size) throw new ArgumentException($"Byte array must be {size} bytes long.");

            PrimeDensity ret = new();

            ret.start = BitConverter.ToUInt64(bytes, 0);
            ret.length = BitConverter.ToUInt64(bytes, 8);
            ret.count = BitConverter.ToUInt64(bytes, 16);

            return ret;
        }
    }
}
