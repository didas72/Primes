namespace JobManagement
{
    public class PrimeDensity
    {
        public ulong start;
        public ulong length;
        public ulong count;
        public double Density { get { return (double)count / (double)length; } }



        public const int size = 3 * sizeof(ulong);



        public PrimeDensity(ulong start, ulong length) { this.start = start; this.length = length; count = 0; }
        public PrimeDensity(ulong start, ulong length, ulong count) { this.start = start; this.length = length; this.count = count; }
    }
}
