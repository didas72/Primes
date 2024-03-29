﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.IO.Compression;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Files;
using DidasUtils.Extensions;
using DidasUtils.Data;

using Primes.Common;
using Primes.Common.Files;
using Primes.Common.Net;

using JobManagement.Stats;

namespace JobManagement
{
    class Program
    {
        //Here goes code that will only get executed a few times for testing purpose and will never be used again.
        //Please ignore this project.

        public const string basePath = "D:\\Primes\\working\\";
        public const ulong perJob = 10000000;

        public static List<string> prints = new();
        private static ScanResults results;

        private readonly static Task todo = Task.Temporary4;



        private static void Main()
        {
            Log.InitLog("E:\\Documents\\primes\\", "JobManagement.txt");

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

                case Task.Temporary1:
                    Temporary1();
                    break;

                case Task.Temporary2:
                    Temporary2();
                    break;

                case Task.Temporary3:
                    Temporary3();
                    break;

                case Task.Temporary4:
                    Temporary4();
                    break;

                default:
                    Red("Invalid/Unhandled task");
                    break;
            }

            //end
            Blue("//Done");
            Console.ReadLine();
        }
        


        private static void DoScan()
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
        private static void ProcessScanResults()
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
        private static void TestCorrection()
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
        private static void Temporary()
        {
            Console.WriteLine("Sleeping 2s...");
            Thread.Sleep(2000);
            Console.WriteLine("Starting...");

            uint clientId = 0;
            TcpClient cli = new();

            try
            {
                cli.Connect("127.0.0.1", 13032);
                if (!cli.Connected) throw new Exception("Failed connection");
                Socket soc = cli.Client;
                NetworkStream ns = cli.GetStream();

                if (!MessageBuilder.ReceiveMessage(ns, out byte[] msg, TimeSpan.FromSeconds(1))) throw new Exception("Failed to get intent request message.");
                MessageBuilder.DeserializeMessage(msg, out string msgType, out string tgt, out object value);
                if (!MessageBuilder.ValidateRequestMessage(msgType, tgt, value)) throw new Exception("Unexpected");
                if ((string)value != "intent") throw new Exception("Unexpcted");

                MessageBuilder.SendMessage(MessageBuilder.Message("ret", string.Empty, $"get;{clientId}"), ns);

                if (!MessageBuilder.ReceiveMessage(ns, out msg, TimeSpan.FromSeconds(1))) throw new Exception("Failed to get server response.");
                MessageBuilder.DeserializeMessage(msg, out msgType, out tgt, out value); Console.WriteLine($"{msgType}:{tgt}:{value}");
                if (MessageBuilder.ValidateErrorMessage(msgType, tgt, value))
                {
                    string[] parts = ((string)value).Split(";");
                    if (parts.Length != 2) throw new Exception("Recieved an invalid error message.");
                    clientId = uint.Parse(parts[0]);
                    Console.WriteLine("New/Same id: " + clientId);
                    Log.LogEvent($"Batch server denied new batch. Reason: {parts[1]}", "GetBatch");
                    cli.Close();
                    if (parts[1] == "NoAvailableBatches")
                        throw new Exception("No avialable batches");
                    else if (parts[1] == "LimitReached")
                        throw new Exception("Limit reached");
                    else
                        throw new Exception("Unspecified");
                }
                else if (!MessageBuilder.ValidateDataMessage(msgType, tgt, value)) throw new Exception($"Unexpected1 {msgType}:{tgt}:{value==null}");

                //generated batches are relatively small (~10k) so it's not too bad to use the normal messaging system
                byte[] bytes = (byte[])value;
                File.WriteAllBytes(Path.Combine(Environment.CurrentDirectory, "batchDownload.7z.tmp"), bytes); //extraction is done externally

                MessageBuilder.SendMessage(MessageBuilder.Message("ack", string.Empty, string.Empty), ns);
                ns.Close();
                cli.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get new batch: " + e);
                try { cli?.Close(); } catch { }
            }

            Console.WriteLine("Success?");
        }
        private static void Temporary1()
        {
            const float total = 406560454; //~387M (knownPrimes.rsrc)
            const string src = "knownPrimes.rsrc";

            //Benchmark builtin vs DidasUtils mapped 7z vs lz4
            FileStream fs;
            FileStream cs;
            Stopwatch sw = new();

            #region 7z
            Console.WriteLine("DU 7z Fast");
            sw.Start();
            SevenZip.Compress7z(src, "TestRDM.7z1", 4, 1);
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.7z1").Length / total}");


            Console.WriteLine("DU 7z Normal");
            sw.Restart();
            SevenZip.Compress7z(src, "TestRDM.7z5", 4, 5);
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.7z5").Length / total}");


            Console.WriteLine("DU 7z Smallest size");
            sw.Restart();
            SevenZip.Compress7z(src, "TestRDM.7z9", 4, 9);
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.7z9").Length / total}");
            #endregion

            #region builtin
            Console.WriteLine("Brotli Fast");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.brt0");
            BrotliStream bs = new(cs, CompressionLevel.Fastest);
            fs.CopyTo(bs);
            bs.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.brt0").Length / total}");


            Console.WriteLine("Brotli Normal");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.brt5");
            bs = new(cs, CompressionLevel.Optimal);
            fs.CopyTo(bs);
            bs.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.brt5").Length / total}");


            Console.WriteLine("Brotli Smallest size");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.brt9");
            bs = new(cs, CompressionLevel.SmallestSize);
            fs.CopyTo(bs);
            bs.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.brt9").Length / total}");


            Console.WriteLine("GZip Fastest");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.gz1");
            GZipStream gs = new(cs, CompressionLevel.Fastest);
            fs.CopyTo(gs);
            gs.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.gz1").Length / total}");


            Console.WriteLine("GZip Normal");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.gz5");
            gs = new(cs, CompressionLevel.Optimal);
            fs.CopyTo(gs);
            gs.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.gz5").Length / total}");


            Console.WriteLine("GZip Smallest size");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.gz9");
            gs = new(cs, CompressionLevel.SmallestSize);
            fs.CopyTo(gs);
            gs.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.gz9").Length / total}");


            Console.WriteLine("Deflate Fastest");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.df1");
            DeflateStream ds = new(cs, CompressionLevel.Fastest);
            fs.CopyTo(ds);
            ds.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.df1").Length / total}");


            Console.WriteLine("Deflate Normal");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.df5");
            ds = new(cs, CompressionLevel.Fastest);
            fs.CopyTo(ds);
            ds.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.df5").Length / total}");


            Console.WriteLine("Deflate Smallest size");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.df9");
            ds = new(cs, CompressionLevel.Fastest);
            fs.CopyTo(ds);
            ds.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.df9").Length / total}");


            Console.WriteLine("Zlib Fastest");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.zl1");
            ZLibStream zs = new(cs, CompressionLevel.Fastest);
            fs.CopyTo(zs);
            zs.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.zl1").Length / total}");


            Console.WriteLine("Zlib Normal");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.zl5");
            zs = new(cs, CompressionLevel.Optimal);
            fs.CopyTo(zs);
            zs.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.zl5").Length / total}");


            Console.WriteLine("Zlib Smallest size");
            sw.Restart();
            fs = File.OpenRead(src); cs = File.OpenWrite("TestRDM.zl9");
            zs = new(cs, CompressionLevel.SmallestSize);
            fs.CopyTo(zs);
            zs.Dispose(); cs.Dispose(); fs.Dispose();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.zl9").Length / total}");
            #endregion

            #region Lz4
            Console.WriteLine("Lz4 Normal");
            sw.Restart();
            Process.Start("inc/lz4.exe", $"{src} TestRDM.lz4").WaitForExit();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.lz4").Length / total}");

            Console.WriteLine("Lz4 Smalles size");
            sw.Restart();
            Process.Start("inc/lz4.exe", $"-9 {src} TestRDM.lzh").WaitForExit();
            sw.Stop();
            Console.WriteLine($"T: {sw.Elapsed.TotalMilliseconds:F2}ms; R: {new FileInfo("TestRDM.lzh").Length / total}");
            #endregion
        }
        private static void Temporary2()
        {
            TestCompressionNew();
        }
        private static void Temporary3()
        {
            string path = Path.Combine(basePath, "redo_complete");

            foreach(string folder in Directory.GetDirectories(path))
            {
                Console.WriteLine($"Checking folder {Path.GetFileName(folder)}...");
                PrimeJob.CheckJobsInFolder(folder, out int good, out int bad);
                Console.WriteLine($"Check complete. {good} good and {bad} bad.");
            }
        }
        private static void Temporary4()
        {
            string path = Path.Combine(basePath, "200\\");
            int[] offsetCounts = new int[0x10000];
            byte[] bytes = new byte[2]; ushort offset;

            foreach (string file in Directory.GetFiles(path))
            {
                FileStream fs = File.OpenRead(file);
                fs.Seek(40, SeekOrigin.Begin);

                while (fs.Read(bytes, 0, 2) == 2)
                {
                    offset = BitConverter.ToUInt16(bytes, 0);

                    /*if (offset == 0)
                    {
                        Console.WriteLine($"Dafuq? '{Path.GetFileName(file)}' @{fs.Position - 2}");
                        return;
                    }*/

                    offsetCounts[offset]++;
                }

                fs.Dispose();
            }

            for (int i = 0; i < offsetCounts.Length; i++)
                if (offsetCounts[i] != 0)
                    Console.WriteLine($"Offset: {i}; Count: {offsetCounts[i]}");
        }



        private static void TestCompressionNew()
        {
            PrimeJob job = PrimeJob.Deserialize(Path.Combine(basePath, "0.primejob"));

            Console.WriteLine($"First {job.Primes[0]} sec {job.Primes[1]}");

            //get min and max diffs
            ulong minDiff = ulong.MaxValue, maxDiff = 0;

            for (int i = 1; i < job.Primes.Count; i++)
            {
                ulong diff = job.Primes[i] - job.Primes[i - 1];

                if (diff > maxDiff) maxDiff = diff;
                if (diff < minDiff) minDiff = diff;
            }

            //get diff range
            ulong diffRange = maxDiff - minDiff;

            Console.WriteLine($"Min {minDiff} max {maxDiff} range {diffRange}");

            ulong validDiffCount = 0;
            bool includeDiff1 = false;

            //either this or include special case for 2
            if (minDiff == 1)
            {
                validDiffCount++;
                diffRange--;
                includeDiff1 = true;
            }

            validDiffCount += diffRange / 2;

            Console.WriteLine($"Valid diff count {validDiffCount}");

            //check how many bits for the diffRange
            int bits = 1;

            while (bits < 64)
            {
                if ((1ul << bits) >= validDiffCount)
                    break;

                bits++;
            }

            Console.WriteLine($"Needs {bits} bits to reach up to {1ul << bits} diffs of {validDiffCount} diffs");

            //create compression flags
            //b7    - reserved (0)
            //b6    - include special case 1 (1 = include)
            //b5->0 - bits per diff - 1 (0x3F = 64; 0x00 = 1)
            byte compressionFlags = (byte)(bits - 1 | (includeDiff1 ? 0x40 : 0x00));

            BitList outp = new();

            outp.AddByte(compressionFlags);

            //compress diffs
            bool[] tmp = new bool[bits];
            for (int i = 1; i < job.Primes.Count; i++)
            {
                ulong diff = job.Primes[i] - job.Primes[i - 1];

                for (int b = 0; b < bits; b++)
                    tmp[b] = (diff & 1ul << b) != 0;

                outp.AddRange(tmp);
            }

            Console.WriteLine($"Compression complete for a total of {outp.Count} bits ({Mathf.DivideRoundUp(outp.Count, 8) / 1024} kB)");
        }
        private static void TestCoreNCC_Uncompress()
        {
            ulong[] ul = new ulong[664579];
            Stopwatch sw = new();

            sw.Start();
            int err = CoreWrapper.NCC_Uncompress("D:\\Primes\\working\\0.primejob", ul, (ulong)ul.LongLength, 32);
            sw.Stop();
            Console.WriteLine($"C: {sw.ElapsedMilliseconds}ms {ul.Length} {err}");

            sw.Restart();
            PrimeJob pj = PrimeJob.Deserialize("D:\\Primes\\working\\0.primejob");
            sw.Stop();
            Console.WriteLine($"C#: {sw.ElapsedMilliseconds}ms {pj.Primes.Count}");

            sw.Restart();
            err = CoreWrapper.NCC_Uncompress("D:\\Primes\\working\\0.primejob", ul, (ulong)ul.LongLength, 32);
            sw.Stop();
            Console.WriteLine($"C: {sw.ElapsedMilliseconds}ms {ul.Length} {err}");

            sw.Restart();
            pj = PrimeJob.Deserialize("D:\\Primes\\working\\0.primejob");
            sw.Stop();
            Console.WriteLine($"C#: {sw.ElapsedMilliseconds}ms {pj.Primes.Count}");

            sw.Restart();
            err = CoreWrapper.NCC_Uncompress("D:\\Primes\\working\\0.primejob", ul, (ulong)ul.LongLength, 32);
            sw.Stop();
            Console.WriteLine($"C: {sw.ElapsedMilliseconds}ms {ul.Length} {err}");

            sw.Restart();
            pj = PrimeJob.Deserialize("D:\\Primes\\working\\0.primejob");
            sw.Stop();
            Console.WriteLine($"C#: {sw.ElapsedMilliseconds}ms {pj.Primes.Count}");

            sw.Restart();
            err = CoreWrapper.NCC_Uncompress("D:\\Primes\\working\\0.primejob", ul, (ulong)ul.LongLength, 32);
            sw.Stop();
            Console.WriteLine($"C: {sw.ElapsedMilliseconds}ms {ul.Length} {err}");

            sw.Restart();
            pj = PrimeJob.Deserialize("D:\\Primes\\working\\0.primejob");
            sw.Stop();
            Console.WriteLine($"C#: {sw.ElapsedMilliseconds}ms {pj.Primes.Count}");

            sw.Restart();
            err = CoreWrapper.NCC_Uncompress("D:\\Primes\\working\\0.primejob", ul, (ulong)ul.LongLength, 32);
            sw.Stop();
            Console.WriteLine($"C: {sw.ElapsedMilliseconds}ms {ul.Length} {err}");

            sw.Restart();
            pj = PrimeJob.Deserialize("D:\\Primes\\working\\0.primejob");
            sw.Stop();
            Console.WriteLine($"C#: {sw.ElapsedMilliseconds}ms {pj.Primes.Count}");

            sw.Restart();
            err = CoreWrapper.NCC_Uncompress("D:\\Primes\\working\\0.primejob", ul, (ulong)ul.LongLength, 32);
            sw.Stop();
            Console.WriteLine($"C: {sw.ElapsedMilliseconds}ms {ul.Length} {err}");

            sw.Restart();
            pj = PrimeJob.Deserialize("D:\\Primes\\working\\0.primejob");
            sw.Stop();
            Console.WriteLine($"C#: {sw.ElapsedMilliseconds}ms {pj.Primes.Count}");

            sw.Restart();
            err = CoreWrapper.NCC_Uncompress("D:\\Primes\\working\\0.primejob", ul, (ulong)ul.LongLength, 32);
            sw.Stop();
            Console.WriteLine($"C: {sw.ElapsedMilliseconds}ms {ul.Length} {err}");

            sw.Restart();
            pj = PrimeJob.Deserialize("D:\\Primes\\working\\0.primejob");
            sw.Stop();
            Console.WriteLine($"C#: {sw.ElapsedMilliseconds}ms {pj.Primes.Count}");

            for (int i = 0, o = 0; i < ul.Length; i++)
            {
                if (ul[i] != pj.Primes[i])
                {
                    Console.WriteLine($"Diff {ul[i]} != {pj.Primes[i]} at {i}");
                    if (++o >= 10)
                        break;
                }
            }
        }




#if false

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
                PrimesMath.IsPrime(i, knownPrimes);
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
#endif

#if false
        private static void TestFermatSpeed()
        {
            FileStream fs = File.OpenRead("D:\\Primes\\working\\rsrc\\knownPrimes.rsrc");
            KnownPrimesResourceFile kprf = KnownPrimesResourceFile.Deserialize(fs);
            fs.Dispose();

            ulong count = 1024ul * 16;//1024ul * 1024;
            ulong start = 1000000000000000;//1024ul * 1024 * 1024 * 128;

            bool[] isPrimeF = new bool[count];
            bool[] isPrimeN = new bool[count];

            long nMs, fMs;

            Stopwatch sw = new();

            Thread.Sleep(100);

            sw.Start();
            for (ulong i = 0; i < count; i++)
            {
                isPrimeF[i] = CompoundIsPrime2Res(i + start, kprf.Primes);
            }
            sw.Stop();
            fMs = sw.ElapsedMilliseconds;

            sw.Restart();
            for (ulong i = 0; i < count; i++)
            {
                isPrimeN[i] = PrimesMath.IsPrime(i + start, kprf.Primes);
            }
            sw.Stop();
            nMs = sw.ElapsedMilliseconds;

            int falsePos = 0, falseNeg = 0;

            for (ulong i = 0; i < count; i++)
            {
                if (isPrimeF[i] != isPrimeN[i])
                {
                    if (isPrimeF[i]) falsePos++;
                    else falseNeg++;
                }
            }

            Console.WriteLine($"Had {falseNeg} false negatives and {falsePos} false positives.");
            Console.WriteLine($"Resrouced elapsed {nMs}ms.");
            Console.WriteLine($"Fermat&Resourced elapsed {fMs}ms.");
            Console.WriteLine($"Fermat was {(nMs / (float)fMs):F2}x faster.");
        }
        private static bool IsPrime(ulong num)
        {
            //a^p [mod p]

            UInt128 p = (UInt128)num;
            UInt128 a_preserve = 2;
            UInt128 a = a_preserve;
            UInt128 cur = a;
            num--;

            while (num != 0)
            {
                if ((num & 1) != 0)
                {
                    cur *= a;
                    cur %= p;
                }

                a *= a;
                a %= p;
                num >>= 1;

                if (cur == 0) return false; //OPTIMIZATION
            }
            
            //Console.WriteLine($"Cur: {cur}");

            return cur == a_preserve;
        }
        private static bool IsPrime2(ulong num)
        {
            //a^p [mod p]

            UInt128 p = (UInt128)num;
            UInt128 a_preserve = 2;
            UInt128 a = a_preserve;
            UInt128 cur = a;
            num--;
            num--; //OPTIMIZATION 2

            while (num != 0)
            {
                if ((num & 1) != 0)
                {
                    cur *= a;
                    cur %= p;
                }

                a *= a;
                a %= p;
                num >>= 1;

                if (cur == 0) return false; //OPTIMIZATION 1
            }

            //Console.WriteLine($"Cur: {cur}");

            //return cur == a_preserve;
            return cur == 1; //OPTIMIZATION 2
        }
        private static bool CompoundIsPrime(ulong num)
        {
            if (IsPrime(num))
                return PrimesMath.IsPrime(num);

            return false;
        }
        private static bool CompoundIsPrime2(ulong num)
        {
            if (IsPrime2(num))
                return PrimesMath.IsPrime(num);

            return false;
        }
        private static bool CompoundIsPrime2Res(ulong num, ulong[] res)
        {
            if (IsPrime2(num))
                return PrimesMath.IsPrime(num, res);

            return false;
        }
        private static UInt128 Exp2(int num)
        {
            //2^1 = 2
            //2^2 = 4
            //2^3 = 2^2 * 2 = 8
            //2^5 = (2^4) * 2 = ((2^2)^2) * 2
            //2^11 = (2^10) * 2 = ((2^5)^2) * 2 = (((2^4) * 2)^2) * 2 = ((((2^2)^2) * 2)^2) * 2

            UInt128 a = 2;
            UInt128 cur = 2;
            num--;

            while (num != 0)
            {
                if ((num & 1) != 0)
                    cur *= a;

                a *= a;
                num >>= 1;
            }

            return cur;
        }
#endif


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
            Temporary1,
            Temporary2,
            Temporary3,
            Temporary4,
            TestCorrection,
        }
    }
}
