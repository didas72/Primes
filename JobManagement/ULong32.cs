using System;

namespace JobManagement
{
    readonly struct ULong32 : IEquatable<ULong32>, IEquatable<uint>
    {
        public readonly uint low, high;

        public ULong32(ulong ul)
        {
            low = (uint)(ul & 0xFFFFFFFF);
            high = (uint)((ul >> 32) & 0xFFFFFFFF);
        }
        public ULong32(uint low, uint high)
        {
            this.low = low;
            this.high = high;
        }
        public ULong32(byte[] bytes, int start)
        {
            low = (uint)(bytes[start] | (bytes[start + 1] << 8) | (bytes[start + 2] << 16) | (bytes[start + 3] << 24));
            high = (uint)(bytes[start + 4] | (bytes[start + 5] << 8) | (bytes[start + 6] << 16) | (bytes[start + 7] << 24));
        }




        public static bool Smaller(ULong32 a, ULong32 b)
        {
            if (a.high < b.high) return true;
            else if (a.high > b.high) return false;

            return (a.low < b.low);
        }
        public static bool SmallerEq(ULong32 a, ULong32 b)
        {
            if (a.high < b.high) return true;
            else if (a.high > b.high) return false;

            return (a.low <= b.low);
        }
        public static bool Greater(ULong32 a, ULong32 b)
        {
            if (a.high > b.high) return true;
            else if (a.high < b.high) return false;

            return (a.low > b.low);
        }
        public static bool GreaterEq(ULong32 a, ULong32 b)
        {
            if (a.high > b.high) return true;
            else if (a.high < b.high) return false;

            return (a.low >= b.low);
        }



        public static ULong32 And(ULong32 a, ULong32 b) => new(a.low & b.low, a.high & b.high);
        public static ULong32 Or(ULong32 a, ULong32 b) => new(a.low | b.low, a.high | b.high);
        public static ULong32 Xor(ULong32 a, ULong32 b) => new(a.low ^ b.low, a.high ^ b.high);
        public static ULong32 Not(ULong32 a) => new(~a.low, ~a.high);
        public static ULong32 LShift(ULong32 a, int bits)
        {
            /*uint inter = 0, low, high;

            if (bits < 32 && bits > 0)
                inter = a.low >> (32 - bits);

            low = a.low << bits;
            high = inter | (a.high << bits);

            return new(low, high);*/

            if (bits == 0) return a;

            uint inter, low = 0, high = 0;

            if (bits < 32 && bits > 0)
            {
                inter = a.low >> (32 - bits);

                low = a.low << bits;
                high = inter | (a.high << bits);
            }
            else if (bits < 64 && bits >= 32)
            {
                high = a.low << (bits - 32);
            }

            return new(low, high);
        }
        public static ULong32 RShift(ULong32 a, int bits)
        {
            /*uint inter = 0, low, high;

            if (bits < 32 && bits > 0)
                inter = a.high << (32 - bits);

            low = inter | (a.low >> bits);
            high = a.high >> bits;

            return new(low, high);*/

            if (bits == 0) return a;

            uint inter, low = 0, high = 0;

            if (bits < 32 && bits > 0)
            {
                inter = a.high << (32 - bits);

                low = inter | (a.low >> bits);
                high = a.high >> bits;
            }
            else if (bits < 64 && bits >= 32)
            {
                low = high >> (bits - 32);
            }

            return new(low, high);
        }



        public static ULong32 Add(ULong32 a, ULong32 b)
        {
            uint low, high;

            low = a.low + b.low;
            high = a.high + b.high;

            if (low < a.low)
                high++;

            return new(low, high);
        }
        public static ULong32 Subtract(ULong32 a, ULong32 b)
        {
            uint low, high;

            low = a.low - b.low;
            high = a.high - b.high;

            if (low > a.low)
                high--;

            return new(low, high);
        }
        public static ULong32 Multiply(ULong32 a, ULong32 b)
        {
            ULong32 ret = new(0, 0);

            for (int i = 0; i < 64; i++)
            {
                ULong32 shift = (ULong32)1 << i;
                ULong32 and = b & shift;

                if (and != 0)
                {
                    ret += a << i;
                }
            }

            return ret;
        }
        public static ULong32 Divide(ULong32 a, ULong32 b)
        {
            ULong32 ret = new(0, 0);

            for (int i = 63; i >= 0; i--)
            {
                ULong32 shift = b << i;

                if (a >= shift)
                {
                    ret += 1 << i;
                    a -= shift;
                }
            }

            return ret;
        }
        public static ULong32 Remainder(ULong32 a, ULong32 b)
        {
            ULong32 ret = new(0, 0);

            for (int i = 63; i >= 0; i--)
            {
                ULong32 shift = b << i;

                if (a >= shift)
                {
                    ret += 1 << i;
                    a -= shift;
                }
            }

            return a;
        }



        public static ULong32 SquareRootHigh(ULong32 a)
        {
            if (a <= 2)
                return a;

            ULong32 max = new(0, 1), min = new(0, 0), c, c2;

            while (true)
            {
                c = (max + min) / 2;
                c2 = c * c;

                if (c2 < a)
                    min = c;
                else if (c2 > a)
                    max = c;
                else
                    return c;

                if (max - min <= 1)
                    return max;
            }
        }



        public byte[] ToBytes()
        {
            byte[] ret = new byte[8];

            ret[0] = (byte)((low) & 0xFF);
            ret[1] = (byte)((low >> 8) & 0xFF);
            ret[2] = (byte)((low >> 16) & 0xFF);
            ret[3] = (byte)((low >> 24) & 0xFF);
            ret[4] = (byte)((high) & 0xFF);
            ret[5] = (byte)((high >> 8) & 0xFF);
            ret[6] = (byte)((high >> 16) & 0xFF);
            ret[7] = (byte)((high >> 24) & 0xFF);

            return ret;
        }
        public ulong ToUlong()
        {
            return (ulong)(low | ((ulong)high << 32));
        }



        #region Operators
        public static ULong32 operator -(ULong32 a) => a;


        public static ULong32 operator +(ULong32 a, ULong32 b) => Add(a, b);
        public static ULong32 operator -(ULong32 a, ULong32 b) => Subtract(a, b);
        public static ULong32 operator *(ULong32 a, ULong32 b) => Multiply(a, b);
        public static ULong32 operator /(ULong32 a, ULong32 b) => Divide(a, b);
        public static ULong32 operator %(ULong32 a, ULong32 b) => Remainder(a, b);

        public static ULong32 operator +(ULong32 a, uint b) => Add(a, new(b, 0));
        public static ULong32 operator -(ULong32 a, uint b) => Subtract(a, new(b, 0));
        public static ULong32 operator *(ULong32 a, uint b) => Multiply(a, new(b, 0));
        public static ULong32 operator /(ULong32 a, uint b) => Divide(a, new(b, 0));
        public static ULong32 operator %(ULong32 a, uint b) => Remainder(a, new(b, 0));

        public static ULong32 operator +(ULong32 a, int b) => Add(a, new((uint)b, 0));
        public static ULong32 operator -(ULong32 a, int b) => Subtract(a, new((uint)b, 0));
        public static ULong32 operator *(ULong32 a, int b) => Multiply(a, new((uint)b, 0));
        public static ULong32 operator /(ULong32 a, int b) => Divide(a, new((uint)b, 0));
        public static ULong32 operator %(ULong32 a, int b) => Remainder(a, new((uint)b, 0));


        public static ULong32 operator <<(ULong32 a, int b) => LShift(a, b);
        public static ULong32 operator >>(ULong32 a, int b) => RShift(a, b);
        public static ULong32 operator &(ULong32 a, ULong32 b) => And(a, b);
        public static ULong32 operator |(ULong32 a, ULong32 b) => Or(a, b);
        public static ULong32 operator ^(ULong32 a, ULong32 b) => Xor(a, b);
        public static ULong32 operator ~(ULong32 a) => Not(a);

        public static ULong32 operator &(ULong32 a, uint b) => And(a, new(b, 0));
        public static ULong32 operator |(ULong32 a, uint b) => Or(a, new(b, 0));
        public static ULong32 operator ^(ULong32 a, uint b) => Xor(a, new(b, 0));

        public static ULong32 operator &(ULong32 a, int b) => And(a, new((uint)b, 0));
        public static ULong32 operator |(ULong32 a, int b) => Or(a, new((uint)b, 0));
        public static ULong32 operator ^(ULong32 a, int b) => Xor(a, new((uint)b, 0));


        public static bool operator <(ULong32 a, ULong32 b) => Smaller(a, b);
        public static bool operator <=(ULong32 a, ULong32 b) => SmallerEq(a, b);
        public static bool operator >(ULong32 a, ULong32 b) => Greater(a, b);
        public static bool operator >=(ULong32 a, ULong32 b) => GreaterEq(a, b);
        public static bool operator ==(ULong32 a, ULong32 b) => a.Equals(b);
        public static bool operator !=(ULong32 a, ULong32 b) => !a.Equals(b);

        public static bool operator <(ULong32 a, uint b) => Smaller(a, new(b, 0));
        public static bool operator <=(ULong32 a, uint b) => SmallerEq(a, new(b, 0));
        public static bool operator >(ULong32 a, uint b) => Greater(a, new(b, 0));
        public static bool operator >=(ULong32 a, uint b) => GreaterEq(a, new(b, 0));
        public static bool operator ==(ULong32 a, uint b) => a.Equals(b);
        public static bool operator !=(ULong32 a, uint b) => !a.Equals(b);

        public static bool operator <(ULong32 a, int b) => Smaller(a, new((uint)b, 0));
        public static bool operator <=(ULong32 a, int b) => SmallerEq(a, new((uint)b, 0));
        public static bool operator >(ULong32 a, int b) => Greater(a, new((uint)b, 0));
        public static bool operator >=(ULong32 a, int b) => GreaterEq(a, new((uint)b, 0));
        public static bool operator ==(ULong32 a, int b) => a.Equals(b);
        public static bool operator !=(ULong32 a, int b) => !a.Equals(b);
        #endregion

        #region Casts
        public static explicit operator ULong32(int a) => new((uint)a, 0);
        public static explicit operator ULong32(uint a) => new(a, 0);
        public static explicit operator ULong32(long a) => new((ulong)a);
        public static explicit operator ULong32(ulong a) => new(a);

        public static explicit operator int(ULong32 a) => (int)a.ToUlong();
        public static explicit operator uint(ULong32 a) => (uint)a.ToUlong();
        public static explicit operator long(ULong32 a) => (long)a.ToUlong();
        public static explicit operator ulong(ULong32 a) => a.ToUlong();
        #endregion



        //Interfaces
        public bool Equals(ULong32 other) => (low == other.low && high == other.high);
        public bool Equals(uint other)
        {
            return (high == 0 && low == other);
        }
    }
}
