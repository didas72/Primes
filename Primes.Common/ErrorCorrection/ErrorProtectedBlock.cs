using System;

namespace Primes.Common.ErrorCorrection
{
    public class ErrorProtectedBlock
    {
        public byte[] data;
        public byte[] errorProtection;
        public ErrorProtectionType errorProtectionType;



        private ErrorProtectedBlock() { }
        public ErrorProtectedBlock(ErrorProtectionType type, byte[] data, byte[] errorProtection)
        {
            if (data.Length > 32768) throw new ArgumentException("Data must no longer than 32768 bytes (32K).");
            if (errorProtection.Length > 256) throw new ArgumentException("Error protection must no longer than 256 bytes.");

            this.errorProtectionType = type;
            this.data = data;
            this.errorProtection = errorProtection;
        }



        public static byte[] Serialize(ErrorProtectedBlock block)
        {
            byte[] ret = new byte[3 + block.data.Length + block.errorProtection.Length];

            ret[0] = (byte)block.errorProtectionType;
            Array.Copy(BitConverter.GetBytes((ushort)block.data.Length), 0, ret, 1, 2);
            Array.Copy(block.data, 0, ret, 3, block.data.Length);
            Array.Copy(block.errorProtection, 0, ret, 3 + block.data.Length, block.errorProtection.Length);

            return ret;
        }
        public static ErrorProtectedBlock Deserialize(byte[] bytes)
        {
            ErrorProtectedBlock ret = new ErrorProtectedBlock();

            ret.errorProtectionType = (ErrorProtectionType)bytes[0];
            ret.data = new byte[BitConverter.ToUInt16(bytes, 1)];
            Array.Copy(bytes, 3, ret.data, 0, ret.data.Length);

            int len = 0xff;
            switch ((ErrorProtectionType)bytes[0])
            {
                case ErrorProtectionType.None:
                    len = 0;
                    break;

                case ErrorProtectionType.CheckSum8:
                    len = 1;
                    break;

                case ErrorProtectionType.CheckSum16:
                    len = 2;
                    break;

                case ErrorProtectionType.CheckSum32:
                    len = 4;
                    break;

                case ErrorProtectionType.CheckSum64:
                    len = 8;
                    break;

                case ErrorProtectionType.Fletcher16:
                    len = 2;
                    break;

                case ErrorProtectionType.Fletcher32:
                    len = 4;
                    break;
            }

            if (len == 0xff)
                throw new Exception("Invalid error protection type.");

            Array.Copy(bytes, 3 + ret.data.Length, ret.errorProtection, 0, len);

            return ret;
        }



        public enum ErrorProtectionType : byte
        {
            None = 0,
            CheckSum8 = 1,
            CheckSum16 = 2,
            CheckSum32 = 3,
            CheckSum64 = 4,
            Fletcher16 = 5,
            Fletcher32 = 6,
        }
    }
}
