using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

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

            /*List<byte> bs = new List<byte>();
            ulong[] uls = new ulong[] { 1, 3, 5, 67555 };
            Compression.NCC.StreamCompress(ref bs, ref uls);
            foreach (byte b in bs) Console.WriteLine(b.ToString("X2"));
            Console.WriteLine("End");
            ulong[] uls2 = new ulong[] { 67566, 67576 };
            Compression.NCC.StreamCompress(ref bs, ref uls2);
            foreach (byte b in bs) Console.WriteLine(b.ToString("X2"));
            Console.WriteLine("End");*/

            //DoAll();
            Console.WriteLine("Testing");
            DoTest();
            

            Console.WriteLine("//Done");
            Console.ReadLine();
        }

        public static void DoAll()
        {
            FileStream stream = File.Open("E:\\Documents\\primes\\working\\knownPrimes.rsrc", FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            string[] jobs = Utils.SortFiles(Directory.GetFiles("E:\\Documents\\primes\\working", "*.primejob"));

            ulong last = 0;
            int count = 0, countf = 0;



            //add header
            stream.Write(new byte[] { 1, 2, 0, new KnownPrimesResourceFile.Comp(true, false).GetByte(), 0, 0, 0, 0 }, 0, 8);
            stream.Flush();



            foreach (string s in jobs)
            {
                //Console.WriteLine(s);

                int max = int.MaxValue;

                PrimeJob job = PrimeJob.Deserialize(s);

                countf += job.Primes.Count;

                for (int i = 0; i < job.Primes.Count; i++)
                {
                    if (job.Primes[i] > 4294967295)
                    {
                        max = i - 1;
                        countf -= job.Primes.Count - max;
                        break;
                    }
                    else count++;
                }

                ulong[] append = job.Primes.GetRange(0, Math.Min(job.Primes.Count, max)).ToArray();

                Compression.NCC.StreamCompress(stream, append, ref last);
            }


            Console.WriteLine(last);
            Console.WriteLine("Saved");
            Console.WriteLine(count);
            Console.WriteLine(countf);
            stream.Seek(4, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes(count), 0, 4);

            stream.Flush();
            stream.Close();
            stream.Dispose();
        }

        public static void DoTest()
        {
            bool brk2 = false;

            FileStream stream = File.OpenRead("E:\\Documents\\primes\\working\\knownPrimes.rsrc");

            byte[] buffer = new byte[8];
            stream.Read(buffer, 0, 8);

            ulong[] nums = new ulong[BitConverter.ToInt32(buffer, 4)];

            byte[] val = new byte[8];

            stream.Read(val, 0, 8);
            stream.Seek(8, SeekOrigin.Begin);
            


            Console.WriteLine("Uncompressing");

            Stopwatch time = new Stopwatch();
            time.Start();
            Compression.NCC.StreamUncompress(stream, nums);
            time.Stop();

            Console.WriteLine($"Elapsed {time.ElapsedMilliseconds}ms");

            PrimeJob s = PrimeJob.Deserialize("E:\\Documents\\primes\\working\\4290000000.primejob");



            if (nums[nums.Length - 1] != s.Primes[s.Primes.Count - 1])
                brk2 = false;

            Console.WriteLine($"Last is intact: {brk2}");

            Console.WriteLine($"Size in memory is {nums.Length * 8}B or {(nums.Length * 8f) / 1024f}kB or {(nums.Length * 8f) / 1048576f}MB");
        }

        /*
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
        */
    }
}
