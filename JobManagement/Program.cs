using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Primes.Common;
using Primes.Common.Files;
using Primes.Common.ErrorCorrection;

namespace JobManagement
{
    class Program
    {
        public const string basePath = "E:\\Documents\\primes\\working\\";
        public const ulong perJob = 10000000;

        public static List<string> prints = new List<string>();

        private static ScanResults results;

        private readonly static Task todo = Task.TestCorrection;

        static void Main()
        {
            //Here goes code that will only get executed a few times for testing purpose and will never be used again.
            //Please ignore this project.

            //Currently set to scan and update compression

            Blue("Start");
            
            switch(todo)
            {
                case Task.None:
                    break;

                case Task.Scan:
                    DoScan();
                    break;

                case Task.ProcessScanResults:
                    ProcessScanResults();
                    break;

                case Task.Temporary:
                    Temporary();
                    break;

                case Task.TestCorrection:
                    TestCorrection();
                    break;

                default:
                    Red("Invalid/Unhandled task");
                    break;
            }

            //end
            Blue("//Done");
            Console.ReadLine();
        }
        


        public static void DoScan()
        {
            DateTime start = DateTime.Now;



            //setup log
            Log.InitLog(basePath, "scan_log.txt");
            Log.PrintByDefault = false;



            //setup paths
            //string sourcePath = "E:\\Documents\\primes\\working\\tmpsrc";
            string sourcePath = "E:\\Documents\\00_Archieved_Primes\\Completed\\";
            string tmpPath = Path.Combine(basePath, "tmp");
            string destPath = Path.Combine(basePath, "outp");
            string rejectsPath = Path.Combine(basePath, "rejects");



            //run scan async
            Scanner scanner = new Scanner(9);

            Thread thread = new Thread(() => results = scanner.RunScan(sourcePath, tmpPath, destPath, rejectsPath));
            thread.Start();



            //do info updates
            while (thread.IsAlive)
            {
                double tS = scanner.lastScanTime.TotalSeconds * (double)(scanner.batchCount - scanner.currentBatch);
                int ETR_h = (int)(tS / 3600);
                int ETR_m = (int)((tS / 60) % 60);
                int ETR_s = (int)(tS % 60);

                TimeSpan elapsed = DateTime.Now - start;

                Console.Clear();
                White($"Start time: {start.Hour:00}:{start.Minute:00}:{start.Second:00}");
                White($"Elapsed: {elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}");
                White($"Progress: {scanner.currentBatch}/{scanner.batchCount}");
                White($"Last scan time: {scanner.lastScanTime.Hours:00}:{scanner.lastScanTime.Minutes % 60:00}:{scanner.lastScanTime.Seconds % 60:00}");
                White($"ETR: {ETR_h:00}:{ETR_m:00}:{ETR_s:00}");
                White("Logs:");

                for (int i = Mathf.Clamp(prints.Count - 10, 0, prints.Count); i < prints.Count; i++)
                {
                    White(prints[i]);
                }

                Thread.Sleep(6000);
            }

            thread.Join();
            Thread.Sleep(2000);



            //clean up
            Log.LogEvent("Cleaning up...", "Main");
            Utils.DeleteDirectory(tmpPath);
            Log.LogEvent("Done.", "Main");



            //save results
            FileStream stream = File.Open(Path.Combine(destPath, "results.bin"), FileMode.OpenOrCreate, FileAccess.ReadWrite);
            lock (results)
            {
                ScanResults.Serialize(stream, results);
            }
            stream.Flush();
            stream.Close();
        }
        public static void ProcessScanResults()
        {
            string resultsPath = Path.Combine(basePath, "outp\\results.bin");
           
            FileStream s = File.OpenRead(resultsPath);
            ScanResults r = ScanResults.Deserialize(s);

            White($"File stats:");
            White($"Total raw size: {r.TotalRawSize()}");
            White($"Total NCC size: {r.TotalNCCSize()}");
            White($"Total zipped size: {r.TotalZippedSize()}");
            White($"Average raw size: {r.AverageRawSize()}");
            White($"Average NCC size: {r.AverageNCCSize()}");
            White($"Average zipped size: {r.AverageZippedSize()}");
            White($"Average NCC ratio: {r.AverageNCCRatio()}");
            White($"Average zipped ratio: {r.AverageZippedRatio()}");
            White($"");
            White($"Primes stats:");
            White($"Total primes: {r.TotalPrimeCount()}");
            White($"Average primes per file: {r.AveragePrimesPerFile()}");
            White($"Average prime density: {r.AveragePrimeDensity()}");
            White($"Total twin primes: {r.TotalTwinPrimes()}");

            r.PrimeDensities.Sort((PrimeDensity a, PrimeDensity b) => a.start.CompareTo(b.start));

            string densityCSV = "Start\tDensity\n";

            for (int i = 0; i < 10000; i++)
            {
                densityCSV += $"{r.PrimeDensities[i].start}\t{r.PrimeDensities[i].Density}\n";
            }

            File.WriteAllText(Path.Combine(basePath, "outp\\densities.csv"), densityCSV.TrimEnd('\n').Replace(".", ","));
        }
        public static void Temporary()
        {
            ulong[] knownPrimes = KnownPrimesResourceFile.Deserialize("E:\\Documents\\primes\\resources\\knownPrimes.rsrc").Primes;

            Stopwatch s = new Stopwatch();

            Thread a, b, c;

            ulong start;
            ulong end;

            Green("10^6");
            start = 1000000;
            end = 1010000;

            a = new Thread(() => BenchFuckingMarkAntS(start, end)); a.Start();
            b = new Thread(() => BenchFuckingMarkSimple(start, end)); b.Start();
            c = new Thread(() => BenchFuckingMarkResource(start, end, ref knownPrimes)); c.Start();

            a.Join(); b.Join(); c.Join();



            Green("10^12");
            start = 1000000000000;
            end = 1000000010000;

            a = new Thread(() => BenchFuckingMarkAntS(start, end)); a.Start();
            b = new Thread(() => BenchFuckingMarkSimple(start, end)); b.Start();
            c = new Thread(() => BenchFuckingMarkResource(start, end, ref knownPrimes)); c.Start();

            a.Join(); b.Join(); c.Join();



            Green("10^15");
            start = 1000000000000000;
            end = 1000000000010000;

            a = new Thread(() => BenchFuckingMarkAntS(start, end)); a.Start();
            b = new Thread(() => BenchFuckingMarkSimple(start, end)); b.Start();
            c = new Thread(() => BenchFuckingMarkResource(start, end, ref knownPrimes)); c.Start();

            a.Join(); b.Join(); c.Join();



            Green("10^16");
            start = 100000000000000000;
            end = 100000000000010000;

            a = new Thread(() => BenchFuckingMarkAntS(start, end)); a.Start();
            b = new Thread(() => BenchFuckingMarkSimple(start, end)); b.Start();
            c = new Thread(() => BenchFuckingMarkResource(start, end, ref knownPrimes)); c.Start();

            a.Join(); b.Join(); c.Join();

            Green("10^17");
            start = 1000000000000000000;
            end = 1000000000000010000;

            a = new Thread(() => BenchFuckingMarkAntS(start, end)); a.Start();
            b = new Thread(() => BenchFuckingMarkSimple(start, end)); b.Start();
            c = new Thread(() => BenchFuckingMarkResource(start, end, ref knownPrimes)); c.Start();

            a.Join(); b.Join(); c.Join();

            Green("10^18");
            start = 1000000000000000000;
            end = 1000000000000010000;

            a = new Thread(() => BenchFuckingMarkAntS(start, end)); a.Start();
            b = new Thread(() => BenchFuckingMarkSimple(start, end)); b.Start();
            c = new Thread(() => BenchFuckingMarkResource(start, end, ref knownPrimes)); c.Start();

            a.Join(); b.Join(); c.Join();
        }
        public static void TestCorrection()
        {
            PrimeJob job = PrimeJob.Deserialize(Path.Combine(basePath, "rejects\\8030000000.primejob.rejected"));
            bool fresh = true;

            ulong current, last = job.Primes[0];
            int corrIndex = 0;
            White($"First: {job.Primes[0]}");

            if (last % 2 == 0)
            {
                Red("RIP");
                return;
            }

            for (int i = 1; i < job.Primes.Count; i++)
            {
                current = job.Primes[i];

                if (current % 2 == 0)
                {
                    if (fresh)
                    {
                        fresh = false;
                        corrIndex = i;
                        Red($"Even at {i} of {job.Primes.Count}");

                        for (ulong v = last + 1; v < current + (ulong)uint.MaxValue; v++)
                        {
                            if (Mathf.IsPrime(v))
                            {
                                Red($"Previous is {last}");
                                Red($"Value should be {v} and is {current}");
                                Red($"Next value is {job.Primes[i+1]}");
                                return;
                                break;
                            }
                        }
                    }
                }
            }
        }



        #region Benchmarking
        private static void BenchFuckingMarkAntS(ulong start, ulong end)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            for (ulong i = start; i < end; i++)
                IsPrime_AntunesSenior(i);
            s.Stop();
            White($"AntS:            {s.ElapsedMilliseconds}ms");
        }
        private static void BenchFuckingMarkSimple(ulong start, ulong end)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            for (ulong i = start; i < end; i++)
                Mathf.IsPrime(i);
            s.Stop();
            White($"Ours (simple):   {s.ElapsedMilliseconds}ms");
        }
        private static void BenchFuckingMarkResource(ulong start, ulong end, ref ulong[] knownPrimes)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            for (ulong i = start; i < end; i++)
                Mathf.IsPrime(i, ref knownPrimes);
            s.Stop();
            White($"Ours (reosurce): {s.ElapsedMilliseconds}ms");
        }
        #endregion



        //By ReccaGithub
        public static bool IsPrime_AntunesSenior(ulong value)
        {
            if (value % 3 == 0)
                return false;

            ulong n, j = 1;
            ulong sqrt = Mathf.UlongSqrtHigh(value);

            while (true)
            {
                n = (6 * j - 1);

                if (n > sqrt || value % n == 0)
                    break;

                n = (6 * j + 1);

                if (n > sqrt || value % n == 0)
                    break;

                j++;
            }

            if (n > sqrt)
                return true;

            return false;
        }



        #region Prints
        public static void Print(string msg)
        {
            lock (prints)
            {
                prints.Add(msg);
            }
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
        #endregion



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



        private enum Task
        {
            None,
            Scan,
            ProcessScanResults,
            GenerateBatches,
            Temporary,
            TestCorrection,
        }
    }
}
