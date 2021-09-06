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

            Random rdm = new Random();

            for (int i = 0; i < 1000000; i++)
            {
                ulong src = (ulong)((double)rdm.Next() * (double)rdm.Next());
                ulong sysSqrt = (ulong)Math.Ceiling(Math.Sqrt(src));
                ulong primesSqrt = Mathf.UlongSqrtHigh((ulong)src);

                if (sysSqrt != primesSqrt)
                    Red($"Failed at {i}: sqrt({src}) = {sysSqrt} while UlongSqrtHigh() = {primesSqrt}");
            }

            Blue("//Done");
            Console.ReadLine();
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

            Compress7z(Path.Combine(basePath, "gen", batch.ToString()), Path.Combine(basePath, "packed", $"{batch}.7z"));
        }


        public static void PatchFiles()
        {
            Thread[] threads = new Thread[12];

            Queue<string> pending = new Queue<string>(Directory.GetDirectories("E:\\Documents\\primes\\working\\1unpacked"));

            while (pending.Count != 0)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    if (threads[i] == null || !threads[i].IsAlive)
                    {
                        string name = Path.GetFileNameWithoutExtension(pending.Dequeue());

                        threads[i] = new Thread(() => PatchJobBatch(name));
                        threads[i].Start();

                        Console.WriteLine($"Thread {i} started with batch {name}");

                        break;
                    }
                }

                Thread.Sleep(200);
            }
        }
        public static void PatchJobBatch(string name)
        {
            string unpackedPath = Path.Combine(basePath, "1unpacked", name);
            string cleanedPath = Path.Combine(basePath, "2cleaned", name);
            string packedPath = Path.Combine(basePath, "3packed", name);


            Directory.CreateDirectory(Path.Combine(basePath, "2cleaned", name));


            foreach (string p in Directory.GetFiles(unpackedPath, "*.primejob"))
            {
                PatchJobFile(name, Path.GetFileNameWithoutExtension(p));
            }


            Compress7z(cleanedPath, packedPath);
        }
        public static void PatchJobFileF(string batchName, string jobName)
        {
            byte[] srcBytes = File.ReadAllBytes(Path.Combine(basePath, "1unpacked", batchName, jobName + ".primejob"));
            byte[] fBytes = new byte[32];
            byte[] rawPrimes = new byte[srcBytes.Length - 35];
            ulong[] primes = new ulong[rawPrimes.Length / 8];



            fBytes[0] = 1; fBytes[1] = 2; fBytes[2] = 0;                            //set version
            Array.Copy(srcBytes, 3, fBytes, 4, 4);                                  //copy batch into correct place
            fBytes[3] = 0b00000010;                                                 //set compression
            Array.Copy(srcBytes, 7, fBytes, 8, 24);                                 //copy start, count and progress



            Array.Copy(srcBytes, 35, rawPrimes, 0, srcBytes.Length - 35);           //copy primes for compression
            Buffer.BlockCopy(rawPrimes, 0, primes, 0, rawPrimes.Length);            //compress


            List<byte> bytes = fBytes.ToList();
            bytes.AddRange(Compression.NCC.Compress(primes));                       //add primes



            File.WriteAllBytes(Path.Combine(basePath, "2cleaned", batchName, jobName + ".primejob"), bytes.ToArray());

            PrimeJob job = PrimeJob.Deserialize(Path.Combine(basePath, "2cleaned", batchName, jobName + ".primejob"));
            PrimeJob.CheckJob(job, true, out _);
            PrimeJob.Serialize(job, Path.Combine(basePath, "2cleaned", batchName, jobName + ".primejob"));
        }
        public static void PatchJobFile(string batchName, string jobName)
        {
            byte[] srcBytes = File.ReadAllBytes(Path.Combine(basePath, "1unpacked", batchName, jobName + ".primejob"));

            PrimeJob.Version ver = PrimeJob.Version.Latest;
            PrimeJob.Comp comp = PrimeJob.Comp.Default;
            uint batch = BitConverter.ToUInt32(srcBytes, 3);
            ulong start = BitConverter.ToUInt64(srcBytes, 7);
            ulong count = BitConverter.ToUInt64(srcBytes, 15);
            ulong progress = BitConverter.ToUInt64(srcBytes, 23);

            ulong[] primes = new ulong[(srcBytes.Length - 35) / 8];
            Buffer.BlockCopy(srcBytes, 35, primes, 0, primes.Length * 8);

            PrimeJob job = new PrimeJob(ver, comp, batch, start, count, progress, ref primes);
            PrimeJob.Serialize(job, Path.Combine(basePath, "2cleaned", batchName, jobName + ".primejob"));
        }



        public static void UpdateFiles()
        {
            Thread[] threads = new Thread[6];

            Queue<string> pending = new Queue<string>(Directory.GetFiles("E:\\Documents\\primes\\working\\0source", "*.7z"));

            while (pending.Count != 0)
            {
                for (int i = 0; i < threads.Length; i++)
                {
                    if (threads[i] == null || !threads[i].IsAlive)
                    {
                        string name = Path.GetFileNameWithoutExtension(pending.Dequeue());

                        threads[i] = new Thread(() => UpdateJobBatch(name));
                        threads[i].Start();

                        Console.WriteLine($"Thread {i} started with batch {name}");

                        break;
                    }
                }

                Thread.Sleep(200);
            }
        }
        public static void UpdateJobBatch(string name)
        {
            string unpackedPath = Path.Combine(basePath, "1unpacked");
            string packedPath = Path.Combine(basePath, "3packed", name);

            Uncompress7z(Path.Combine(basePath, "0source", name +  ".7z"), unpackedPath);
            Directory.CreateDirectory(Path.Combine(basePath, "2cleaned", name));

            foreach (string p in Directory.GetFiles(Path.Combine(unpackedPath, name), "*.primejob"))
            {
                UpdateJobFile(name, Path.GetFileNameWithoutExtension(p));
            }

            Compress7z(Path.Combine(basePath, "2cleaned", name), packedPath);
        }
        public static void UpdateJobFile(string batchName, string jobName)
        {
            PrimeJob job = PrimeJob.Deserialize(Path.Combine(basePath, "1unpacked", batchName, jobName + ".primejob"));

            PrimeJob.CheckJob(job, true, out string log);

            if (log.Length != 0)
            {
                Directory.CreateDirectory(Path.Combine(basePath, "logs", batchName));
                File.WriteAllText(Path.Combine(basePath, "logs", batchName, jobName + ".log.txt"), log);
            }

            PrimeJob cleaned = new PrimeJob(new PrimeJob.Version(1, 2, 0), new PrimeJob.Comp(true, false), job.Batch, job.Start, job.Count, job.Progress, job.Primes);

            PrimeJob.Serialize(cleaned, Path.Combine(basePath, "2cleaned", batchName, jobName + ".primejob"));
        }



        public static void UpdateEmptyBatch(string name)
        {
            string unpackedPath = Path.Combine(basePath, "1unpacked");
            string packedPath = Path.Combine(basePath, "3packed", name);

            Directory.CreateDirectory(Path.Combine(basePath, "2cleaned", name));

            foreach (string p in Directory.GetFiles(Path.Combine(unpackedPath, name), "*.primejob"))
            {
                UpdateEmptyJob(name, Path.GetFileNameWithoutExtension(p));
            }

            Compress7z(Path.Combine(basePath, "2cleaned", name), packedPath);
        }
        public static void UpdateEmptyJob(string batchName, string jobName)
        {
            PrimeJob old = PrimeJob.Deserialize(Path.Combine(basePath, "1unpacked", batchName, jobName + ".primejob"));
            PrimeJob newf = new PrimeJob(PrimeJob.Version.Latest, PrimeJob.Comp.Default, old.Batch, old.Start, old.Count, 0, new List<ulong>());

            PrimeJob.Serialize(newf, Path.Combine(basePath, "2cleaned", batchName, jobName + ".primejob"));
        }



        /*public static void DoAll()
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
        */

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

                    if (!PrimeJob.CheckJob(job, true, out string msg))
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
        */

        public static void Compress7z(string sourceDir, string outDir)
        {
            ProcessStartInfo i = new ProcessStartInfo
            {
                FileName = "7za.exe",
                Arguments = $"a {outDir} {sourceDir}",
                WindowStyle = ProcessWindowStyle.Hidden
            };


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
