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

            ulong[] src = new ulong[] { 2, 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };

            byte[] bytes = Compression.Compress(src);

            foreach(byte b in bytes)
            {
                Console.WriteLine(b.ToString("X2"));
            }

            ulong[] ulongs = Compression.Uncompress(bytes);

            for (int i = 0; i < src.Length; i++)
            {
                Console.WriteLine($"{src[i]} u {ulongs[i]}");
            }

            Console.ReadLine();
        }
    }
}
