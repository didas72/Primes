using System;
using System.Diagnostics;
using System.Threading;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace JobManagement
{
    class Program
    {
        static void Main()
        {
            //Here goes code that will only get executed a few times for testing purpose and will never be used again.
            //Please ignore this project.            

            //ulong[] src = new ulong[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 255646 };
            ulong[] src = GetPrimes();

            Console.WriteLine("ONSS");

            byte[] ONSS_bytes = Compression.ONSS.Compress(src);

            ulong[] ONSS_ulongs = Compression.ONSS.Uncompress(ONSS_bytes);

            /*for (int i = 0; i < src.Length; i++)
            {
                Console.WriteLine($"{src[i]} u {ONSS_ulongs[i]}");
            }*/

            for (int i = 0; i < src.Length; i++)
            {
                if (ONSS_ulongs[i] != src[i])
                    Console.WriteLine($"Error at {i}, {ONSS_ulongs[i]} != {src[i]}s");
            }

            Console.WriteLine("NCC");

            byte[] NCC_bytes = Compression.NCC.Compress(src);

            ulong[] NCC_ulongs = Compression.NCC.Uncompress(NCC_bytes);

            /*for (int i = 0; i < src.Length; i++)
            {
                Console.WriteLine($"{src[i]} u {NCC_ulongs[i]}");
            }*/

            for (int i = 0; i < src.Length; i++)
            {
                if (NCC_ulongs[i] != src[i])
                    Console.WriteLine($"Error at {i}, {NCC_ulongs[i]} != {src[i]}s");
            }

            Console.WriteLine($"Source: {src.Length * 8}B");
            Console.WriteLine($"ONSS Compressed: {ONSS_bytes.Length}B");
            Console.WriteLine($"ONSS Compression ratio: {ONSS_bytes.Length * 100f / (src.Length * 8)}%");
            Console.WriteLine($"NCC Compressed: {NCC_bytes.Length}B");
            Console.WriteLine($"NCC Compression ratio: {NCC_bytes.Length * 100f / (src.Length * 8)}%");

            Console.ReadLine();
        }

        private static ulong[] GetPrimes()
        {
            PrimeJob job = PrimeJob.Deserialize("D:\\Documents\\primes\\complete\\71\\700000000000.primejob");

            PrimeJob.CheckJob(ref job, true, out string msg);

            Console.WriteLine($"Check message: '{msg}'");

            return job.Primes.ToArray();
        }
    }
}
