using System;

namespace Primes.BatchDistributer.Files
{
    public struct TimeStamp
    {
        private readonly long stamp;

        public TimeStamp(DateTime time) => stamp = time.ToUniversalTime().ToBinary();
        public TimeStamp(long stamp) => this.stamp = stamp;



        public DateTime GetDateTime() => DateTime.FromBinary(stamp).ToLocalTime();



        public byte[] Serialize() => BitConverter.GetBytes(stamp);
        public static TimeStamp Deserialize(byte[] buffer)
        {
            if (buffer.Length != 8)
                throw new ArgumentException("Buffer size must be 8 bytes.");

            return new TimeStamp(BitConverter.ToInt64(buffer, 0));
        }
    }
}
