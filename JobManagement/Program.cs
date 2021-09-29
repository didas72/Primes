using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using Primes;
using Primes.Common;
using Primes.Common.Files;
using Primes.Common.Net;

namespace JobManagement
{
    class Program
    {
        public const string basePath = "E:\\Documents\\primes\\working\\";
        public const ulong perJob = 10000000;

        static void Main()
        {
            //Here goes code that will only get executed a few times for testing purpose and will never be used again.
            //Please ignore this project.

            KnownPrimesResourceFile file = KnownPrimesResourceFile.Deserialize("C:\\Users\\diogo\\Documents\\primes\\resources\\knownPrimes.rsrc");
            ulong[] primes = file.Primes;

            Stopwatch stopwatch1 = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();
            stopwatch1.Start();

            for (ulong i = 0; i < 100000; i++)
            {
                IsPrime_AntunesSenior(i);
            }

            stopwatch1.Stop();
            stopwatch2.Start();

            for (ulong i = 0; i < 100000; i++)
            {
                Mathf.IsPrime(i, ref primes);
            }

            stopwatch2.Stop();

            Green($"{stopwatch1.ElapsedMilliseconds} and {stopwatch2.ElapsedMilliseconds}");

            Blue("//Done");
            Console.ReadLine();
        }

        //By ReccaGithub
        public static bool IsPrime_AntunesSenior(ulong value)
        {
            if (value % 3 == 0)
                return false;

            ulong n, j = 1;

            while (true)
            {
                n = (6 * j - 1);

                if (n > Mathf.UlongSqrtHigh(value) || value % n == 0)
                    break;

                n = (6 * j + 1);

                if (n > Mathf.UlongSqrtHigh(value) || value % n == 0)
                    break;

                j++;
            }

            if (n > Mathf.UlongSqrtHigh(value))
                return true;

            return false;
        }



        public static void Red(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
        }
        public static void Green(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg);
        }
        public static void Blue(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(msg);
        }
        public static void White(string msg)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(msg);
        }



        public static void GenerateBatches(uint start, uint count)
        {
            for (uint b = start; b < start + count; b++)
            {
                GenerateBatch((ulong)b * (perJob * 1000));
            }
        }
        public static void GenerateBatch(ulong start)
        {
            uint batch = (uint)(start / (perJob * 1000) + 1);

            Directory.CreateDirectory(Path.Combine(basePath, "gen", batch.ToString()));

            PrimeJob job;

            for (ulong i = 0; i < 1000; i++)
            {
                ulong s = start + (i * perJob);

                job = new PrimeJob(PrimeJob.Version.Latest, PrimeJob.Comp.Default, batch, s, perJob);
                PrimeJob.Serialize(job, Path.Combine(basePath, "gen", batch.ToString(), $"{job.Start}.primejob"));
            }

            SevenZip.Compress7z(Path.Combine(basePath, "gen", batch.ToString()), Path.Combine(basePath, "packed", $"{batch}.7z"));
        }
    }
}
