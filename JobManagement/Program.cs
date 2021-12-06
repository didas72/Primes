using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Files;
using DidasUtils.Extensions;

using Primes.Common;
using Primes.Common.Files;

namespace JobManagement
{
    class Program
    {
        public const string basePath = "E:\\Documents\\primes\\working\\";
        public const ulong perJob = 10000000;

        public static List<string> prints = new();

        private static ScanResults results;

        private readonly static Task todo = Task.Temporary;

        private static void Main()
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
            Log.UsePrint = false;



            //setup paths
            //string sourcePath = "E:\\Documents\\primes\\working\\tmpsrc";
            string sourcePath = "E:\\Documents\\00_Archieved_Primes\\Completed\\";
            string tmpPath = Path.Combine(basePath, "tmp");
            string destPath = Path.Combine(basePath, "outp");
            string rejectsPath = Path.Combine(basePath, "rejects");



            //run scan async
            Scanner scanner = new(9);

            Thread thread = new(() => results = scanner.RunScan(sourcePath, tmpPath, destPath, rejectsPath));
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
            Directory.Delete(tmpPath, true);
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
            
        }
        public static void TestCorrection()
        {
            FileStream r = File.OpenRead(Path.Combine(basePath, "rejects\\8030000000.primejob.rejected"));
            FileStream w = File.OpenWrite(Path.Combine(basePath, "rejects\\8030000000.primejob.rejected.fix"));

            ulong last = 0, curr, test, ulDiff;
            ushort diff, lastCorrupt = 0;
            byte[] buff = new byte[32];
            bool bigJump = true, prevBigJump = false;

            r.Seek(0, SeekOrigin.Begin);
            r.Read(buff, 0, 32);
            w.Write(buff, 0, 32);

            while (r.RemainingBytes() >= 2)
            {
                if (bigJump)
                {
                    buff = new byte[8];
                    r.Read(buff, 0, 8);

                    curr = BitConverter.ToUInt64(buff, 0);

                    bigJump = false;
                }
                else
                {
                    buff = new byte[2];
                    r.Read(buff, 0, 2);

                    diff = BitConverter.ToUInt16(buff, 0);

                    if (diff == 0)
                    {
                        bigJump = true;
                        w.Write(buff, 0, 2);
                        continue;
                    }

                    curr = last + diff;
                }

                //Change after functional
                if (lastCorrupt != 0)
                {
                    if (!PrimesMath.IsPrime(curr))
                    {
                        Green("==STACKED CORRUPTION==");

                        FixLocalCorruption(ref last, ref curr, r, w);

                        if (prevBigJump)
                            r.Seek(-10, SeekOrigin.Current);
                        else
                            r.Seek(-2, SeekOrigin.Current);

                        last = curr;
                        lastCorrupt = ushort.MaxValue;
                        continue;
                    }

                    lastCorrupt--;
                }
                //EOC

                if (last >= curr || curr % 2 == 0)
                {
                    lastCorrupt = ushort.MaxValue;
                    test = last + (last % 2 == 0 ? 1ul : 2ul);

                    while (true)
                    {
                        if (PrimesMath.IsPrime(test)) break;

                        test += 2;
                    }

                    ulDiff = test - last;

                    Blue("==ERROR START==");
                    Red($"Error at {r.Position:X8}");
                    Red($"Value {curr} should be {test}");
                    Red($"Long offset is {ulDiff}");

                    if (ulDiff > (ulong)ushort.MaxValue)
                    {
                        w.Write(BitConverter.GetBytes(test), 0, 8);
                    }
                    else
                    {
                        w.Seek(-2, SeekOrigin.Current);
                        w.Write(BitConverter.GetBytes((ushort)ulDiff), 0, 2);
                    }

                    curr = test;
                }
                else
                {
                    w.Write(buff, 0, buff.Length);
                }

                last = curr;
                prevBigJump = bigJump;
            }

            r.Close();
            w.Flush();
            w.Close();
        }
        private static void FixLocalCorruption(ref ulong last, ref ulong curr, Stream r, Stream w)
        {
            ulong test = last + (last % 2 == 0 ? 1ul : 2ul);

            while (true)
            {
                if (PrimesMath.IsPrime(test)) break;

                test += 2;
            }

            ulong ulDiff = test - last;

            Red($"Error at {r.Position:X8}");
            Red($"Value {curr} should be {test}");
            Red($"Long offset is {ulDiff}");

            if (ulDiff > (ulong)ushort.MaxValue)
            {
                w.Write(BitConverter.GetBytes(test), 0, 8);
            }
            else
            {
                w.Seek(-2, SeekOrigin.Current);
                w.Write(BitConverter.GetBytes((ushort)ulDiff), 0, 2);
            }

            curr = test;
        }



        #region Benchmarking
        private static void BenchFuckingMarkAntS(ulong start, ulong end)
        {
            Stopwatch s = new();
            s.Start();
            for (ulong i = start; i < end; i++)
                IsPrime_AntunesSenior(i);
            s.Stop();
            White($"AntS:            {s.ElapsedMilliseconds}ms");
        }
        private static void BenchFuckingMarkSimple(ulong start, ulong end)
        {
            Stopwatch s = new();
            s.Start();
            for (ulong i = start; i < end; i++)
                PrimesMath.IsPrime(i);
            s.Stop();
            White($"Ours (simple):   {s.ElapsedMilliseconds}ms");
        }
        private static void BenchFuckingMarkResource(ulong start, ulong end, ref ulong[] knownPrimes)
        {
            Stopwatch s = new();
            s.Start();
            for (ulong i = start; i < end; i++)
                PrimesMath.IsPrime(i, ref knownPrimes);
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
            ulong sqrt = PrimesMath.UlongSqrtHigh(value);

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
