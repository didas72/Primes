using System;

namespace Primes.BatchDistributer.Files
{
    public struct TimeStamp
    {
        private readonly long stamp;



        public TimeStamp(DateTime time) => stamp = time.ToUniversalTime().ToBinary();
        public TimeStamp(long stamp) => this.stamp = stamp;



        public static TimeStamp Now() => new TimeStamp(DateTime.Now);



        public DateTime GetDateTime() => DateTime.FromBinary(stamp).ToLocalTime();



        public byte[] Serialize() => BitConverter.GetBytes(stamp);
        public static TimeStamp Deserialize(byte[] buffer) => Deserialize(buffer, 0);
        public static TimeStamp Deserialize(byte[] buffer, int startIndex)
        {
            return new TimeStamp(BitConverter.ToInt64(buffer, startIndex));
        }
    }
}
