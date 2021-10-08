namespace JobManagement
{
    public class TwinPrimes
    {
        public ulong first;
        public ulong second;

        public TwinPrimes(ulong first) { this.first = first; second = first + 2; }
        public TwinPrimes(ulong first, ulong second) { this.first = first; this.second = second; }
    }
}
