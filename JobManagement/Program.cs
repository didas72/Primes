using System;
using System.IO;
using System.Diagnostics;



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

            Stopwatch w = new Stopwatch();

            w.Start();

            for (ulong i = 1; i < 100001; i++)
            {
                UlongSqrtHighSPDTEST(i * 163 - 3);
            }

            w.Stop();

            Console.WriteLine(w.ElapsedMilliseconds);
        }

        public static ulong UlongSqrtHighSPDTEST(ulong number)
        {
            if (number < 3)
                return number;

            ulong max = 4294967295, min = 1, c, c2;

            while (true)
            {
                c = min + ((max - min) / 2);
                c2 = c * c;

                if (c2 < number)
                    min = c;
                else if (c2 > number)
                    max = c;
                else
                    return c;

                if (max - min <= 1)
                    return max;
            }
        }

        public static void FixAndUpdate()
        {
            string sourcePath = "D:\\Documents\\primes\\fixing\\source\\";
            string uncompressedPath = "D:\\Documents\\primes\\fixing\\uncompressed\\";
            string cleanedPath = "D:\\Documents\\primes\\fixing\\cleaned\\";
            string finalPath = "D:\\Documents\\primes\\fixing\\final\\";

            string[] sourceFiles = Directory.GetFiles(sourcePath, "*.7z");

            uint passed = 0, failed = 0;

            foreach (string s in sourceFiles)
            {
                string dirName = Path.GetFileNameWithoutExtension(s);

                Console.WriteLine($"Uncompressing. {dirName}");

                Uncompress7z(s, uncompressedPath);

                string[] jobs = Directory.GetFiles(Path.Combine(uncompressedPath, dirName));

                Directory.CreateDirectory(Path.Combine(cleanedPath, dirName));

                Console.WriteLine("Checking and compressing.");

                foreach (string j in jobs)
                {
                    string fileName = Path.GetFileName(j);

                    PrimeJob job = PrimeJob.Deserialize(j);

                    if (!PrimeJob.CheckJob(ref job, true, out string msg))
                    {
                        failed++;
                        Console.WriteLine(msg);
                    }
                    else
                        passed++;

                    PrimeJob newJob = new PrimeJob(PrimeJob.Version.Latest, new PrimeJob.Comp(true, false), job.Batch, job.Start, job.Count, job.Progress, job.Primes);

                    PrimeJob.Serialize(ref newJob, Path.Combine(cleanedPath, dirName, fileName));
                }

                Console.WriteLine("Compressing.");

                Compress7z(Path.Combine(cleanedPath, dirName + "\\"), Path.Combine(finalPath, dirName + ".7z"));

                Console.WriteLine("Cleaning.");

                Directory.Delete(Path.Combine(uncompressedPath, dirName), true);
                Directory.Delete(Path.Combine(cleanedPath, dirName), true);
            }

            Console.WriteLine($"Done. {passed} passed and {failed} failed.");

            Console.ReadLine();
        }
		
        public static void Compress7z(string sourceDir, string outDir)
        {
            ProcessStartInfo i = new ProcessStartInfo
            {
                FileName = "7za.exe",
                Arguments = $"a {outDir} {sourceDir}",
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Console.WriteLine($"a {outDir} {sourceDir}");

            
            Process p = Process.Start(i);
            p.WaitForExit();
        }

        public static void Uncompress7z(string sourceDir, string outDir)
        {
            ProcessStartInfo i = new ProcessStartInfo
            {
                FileName = "7za.exe",
                Arguments = $"x {sourceDir} -o{outDir}",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process p = Process.Start(i);
            p.WaitForExit();
        }
    }
}
