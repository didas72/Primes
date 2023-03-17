using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace JobManagement
{
    public readonly struct UInt256 : INumber<UInt256>
    {
        /// <summary>
        /// Higher 64 bits
        /// </summary>
        private readonly UInt128 a;
        /// <summary>
        /// Lower 64 bits
        /// </summary>
        private readonly UInt128 b;



        #region Constants
        public static UInt256 One => 1;

        public static int Radix => throw new NotImplementedException();

        public static UInt256 Zero => 0;

        public static UInt256 AdditiveIdentity => Zero;

        public static UInt256 MultiplicativeIdentity => One;
        #endregion


        #region Constructors
        public UInt256(UInt128 a, UInt128 b)
        {
            this.a = a; this.b = b;
        }
        public UInt256(UInt128 i)
        {
            a = 0;
            b = i;
        }
        #endregion


        #region Interfaces
        public override int GetHashCode() => HashCode.Combine(a, b);

        static UInt256 INumberBase<UInt256>.Abs(UInt256 value) => value;

        static bool INumberBase<UInt256>.IsCanonical(UInt256 value) => throw new NotImplementedException();

        static bool INumberBase<UInt256>.IsComplexNumber(UInt256 value) => false;

        static bool INumberBase<UInt256>.IsEvenInteger(UInt256 value) => (value % 2) == 0;

        static bool INumberBase<UInt256>.IsFinite(UInt256 value) => true;

        static bool INumberBase<UInt256>.IsImaginaryNumber(UInt256 value) => false;

        static bool INumberBase<UInt256>.IsInfinity(UInt256 value) => false;

        static bool INumberBase<UInt256>.IsInteger(UInt256 value) => true;

        static bool INumberBase<UInt256>.IsNaN(UInt256 value) => false;

        static bool INumberBase<UInt256>.IsNegative(UInt256 value) => false;

        static bool INumberBase<UInt256>.IsNegativeInfinity(UInt256 value) => false;

        static bool INumberBase<UInt256>.IsNormal(UInt256 value) => throw new NotImplementedException();

        static bool INumberBase<UInt256>.IsOddInteger(UInt256 value) => (value % 2) != 0;

        static bool INumberBase<UInt256>.IsPositive(UInt256 value) => value != 0;

        static bool INumberBase<UInt256>.IsPositiveInfinity(UInt256 value) => false;

        static bool INumberBase<UInt256>.IsRealNumber(UInt256 value) => true;

        static bool INumberBase<UInt256>.IsSubnormal(UInt256 value) => throw new NotImplementedException();

        static bool INumberBase<UInt256>.IsZero(UInt256 value) => value.a == 0 && value.b == 0;

        static UInt256 INumberBase<UInt256>.MaxMagnitude(UInt256 x, UInt256 y) => x > y ? x : y;

        static UInt256 INumberBase<UInt256>.MaxMagnitudeNumber(UInt256 x, UInt256 y) => x > y ? x : y;

        static UInt256 INumberBase<UInt256>.MinMagnitude(UInt256 x, UInt256 y) => x < y ? x : y;

        static UInt256 INumberBase<UInt256>.MinMagnitudeNumber(UInt256 x, UInt256 y) => x < y ? x : y;
        #endregion


        #region Comparison
        public override bool Equals(object obj)
        {
            if (obj is UInt256 ui)
                return ui.a == a && ui.b == b;

            return false;
        }

        public int CompareTo(UInt256 other)
        {
            if (a < other.a) return -1;
            if (a > other.b) return 1;
            if (b < other.b) return -1;
            if (b > other.b) return 1;
            return 0;
        }

        bool IEquatable<UInt256>.Equals(UInt256 other) => Equals(other);

        int IComparable.CompareTo(object obj)
        {
            if (obj is not UInt256 ui)
                throw new ArgumentException(nameof(obj));

            return CompareTo(ui);
        }

        public static bool operator ==(UInt256 left, UInt256 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UInt256 left, UInt256 right)
        {
            return !(left == right);
        }

        public static bool operator <(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(UInt256 left, UInt256 right)
        {
            return left.CompareTo(right) >= 0;
        }
        #endregion


        #region Operators
        public static UInt256 operator +(UInt256 value) => new(value.a, value.b);

        public static UInt256 operator -(UInt256 value) => new(-value.a, value.b);

        public static UInt256 operator +(UInt256 left, UInt256 right)
        {
            UInt128 a = left.a + right.a, b = left.b + right.b;

            if (b < left.b)
                a++;

            return new(a, b);
        }

        public static UInt256 operator -(UInt256 left, UInt256 right)
        {
            UInt128 a = left.a - right.a, b = left.b - right.b;

            if (b > left.b)
                a--;

            return new(a, b);
        }

        public static UInt256 operator ++(UInt256 value) => new(value.a + (UInt128)(((value.b + 1) == 0) ? 1 : 0), value.b + 1);

        public static UInt256 operator --(UInt256 value) => new(value.a - (UInt128)(((value.b - 1) > value.b) ? 1 : 0), value.b - 1);

        public static UInt256 operator &(UInt256 left, UInt256 right) => new(left.a & right.a, left.b & right.b);

        public static UInt256 operator |(UInt256 left, UInt256 right) => new(left.a | right.a, left.b | right.b);

        public static UInt256 operator ^(UInt256 left, UInt256 right) => new(left.a ^ right.a, left.b ^ right.b);

        public static UInt256 operator ~(UInt256 value) => new(~value.a, ~value.b);

        public static UInt256 operator <<(UInt256 left, int right)
        {
            if (right >= 256 || right <= 0)
                return left;

            return new((left.a << right) | (left.b << (256 - right)), left.b << right);
        }

        public static UInt256 operator >>(UInt256 left, int right)
        {
            if (right >= 256 || right <= 0)
                return left;

            return new(left.a >> right, (left.a >> (256 - right)) | (left.b >> right));
        }

        public static UInt256 operator *(UInt256 left, UInt256 right)
        {
            UInt256 ret = Zero;

            for (int i = 0; i < 256; i++)
            {
                UInt256 shift = (UInt256)1 << i;

                if ((right & shift) != Zero)
                    ret += left << i;
            }

            return ret;
        }

        public static UInt256 operator /(UInt256 left, UInt256 right)
        {
            UInt256 ret = Zero;

            for (int i = 255; i >= 0; i--)
            {
                UInt256 shift = right << i;

                if (right >= shift)
                {
                    ret += (UInt256)1 << i;
                    left -= shift;
                }
            }

            return ret;
        }

        public static UInt256 operator %(UInt256 left, UInt256 right)
        {
            for (int i = 255; i >= 0; i--)
            {
                UInt256 shift = right << i;

                if (right >= shift)
                    left -= shift;
            }

            return left;
        }
        #endregion


        #region Casting
        public static implicit operator UInt256(int a) => new((UInt128)a);
        public static explicit operator UInt256(uint a) => new((UInt128)a);
        public static explicit operator UInt256(long a) => new((UInt128)a);
        public static explicit operator UInt256(ulong a) => new((UInt128)a);
        #endregion


        #region Parsing
        public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider provider, [MaybeNullWhen(false)] out UInt256 result) => throw new NotImplementedException();

        public static UInt256 Parse(ReadOnlySpan<char> s, IFormatProvider provider) => throw new NotImplementedException();

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, [MaybeNullWhen(false)] out UInt256 result) => throw new NotImplementedException();

        public static UInt256 Parse(string s, IFormatProvider provider) => throw new NotImplementedException();

        public static bool TryParse([NotNullWhen(true)] string s, IFormatProvider provider, [MaybeNullWhen(false)] out UInt256 result) => throw new NotImplementedException();

        static UInt256 INumberBase<UInt256>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider provider) => throw new NotImplementedException();

        static UInt256 INumberBase<UInt256>.Parse(string s, NumberStyles style, IFormatProvider provider) => throw new NotImplementedException();

        static bool INumberBase<UInt256>.TryParse(string s, NumberStyles style, IFormatProvider provider, out UInt256 result) => throw new NotImplementedException();
        #endregion


        #region Converting
        static bool INumberBase<UInt256>.TryConvertFromChecked<TOther>(TOther value, out UInt256 result) => throw new NotImplementedException();

        static bool INumberBase<UInt256>.TryConvertFromSaturating<TOther>(TOther value, out UInt256 result) => throw new NotImplementedException();

        static bool INumberBase<UInt256>.TryConvertFromTruncating<TOther>(TOther value, out UInt256 result) => throw new NotImplementedException();

        static bool INumberBase<UInt256>.TryConvertToChecked<TOther>(UInt256 value, out TOther result) => throw new NotImplementedException();

        static bool INumberBase<UInt256>.TryConvertToSaturating<TOther>(UInt256 value, out TOther result) => throw new NotImplementedException();

        static bool INumberBase<UInt256>.TryConvertToTruncating<TOther>(UInt256 value, out TOther result) => throw new NotImplementedException();
        #endregion


        #region String
        bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider) => throw new NotImplementedException();

        string IFormattable.ToString(string format, IFormatProvider formatProvider) => throw new NotImplementedException();
        #endregion
    }
}