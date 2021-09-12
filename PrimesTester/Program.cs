using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;

using Primes;
using Primes.Common;

namespace Primes.Tester
{
    class Program
    {
        private static string homePath;



        static void Main(string[] args)
        {
            Log.Print("PrimesTexter.exe by Didas72 and PeakRead");

            Init(ref args);

            Log.LogEvent(Log.EventType.Info, $"Options: \n{RunOptions.ExportOptions()}\n", "MainThread", false);

            if (RunOptions.CollectSysInfo)
                CollectSystemInfo();

            if (RunOptions.RunBenchmark)
                RunBenchmark();

            Log.Print("Done. Press any key to exit.");
            Utils.WaitForKey();
        }



        private static void Init(ref string[] args)
        {
            InitDirs();

            InitLog();

            ParseArguments(ref args);
        }
        private static void InitDirs()
        {
            homePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
            Directory.CreateDirectory(homePath);
        }
        private static void InitLog()
        {
            Log.InitConsole();
            Log.InitLog(homePath, "testerLog.txt");
        }
        private static void ParseArguments(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "/?":

                        Log.Print("Simple benchmarking / stress testing tool based on prime checking.");
                        Log.Print("Arguments:");
                        Log.Print("-b          - Runs benchmark.");
                        Log.Print("-b-prime    - Runs prime testing performance tests. Not used by default. (use with /b)");
                        Log.Print("-b-sqrt     - Runs square root performance tests. Not used by default. (use with /b)");
                        Log.Print("-s          - Runs stress test. (Not implemented)");
                        Log.Print("-threads-XX - Runs with XX threads. If not included the program will decide the most appropriate value.");
                        Log.Print("-no-sys-info  - Disables collection of system information. Not set by default.");

                        Environment.Exit(0);

                        break;

                    case "-b":
                        RunOptions.RunBenchmark = true;
                        break;

                    case "-b-prime":
                        RunOptions.RunPrime = true;
                        break;

                    case "-b-sqrt":
                        RunOptions.RunSqrt = true;
                        break;

                    case "-s":
                        RunOptions.RunStressTest = true;
                        break;

                    case "-no-sys-info":
                        RunOptions.CollectSysInfo = false;
                        break;

                    default:

                        if (args[i].StartsWith("-threads-"))
                        {
                            if (!int.TryParse(args[i].Substring(9), out RunOptions.Threads))
                            {
                                Log.Print("Invalid thread count!");
                                Thread.Sleep(2000);
                                Environment.Exit(0);
                            }
                        }
                        else
                            Log.Print("Invalid arg");
                        
                        break;
                }
            }
        }



        private static void CollectSystemInfo()
        {
            try
            {
                Log.Print("Collecting system details...");
                Log.LogEvent(Log.EventType.Info, $"System Info:\n{new InfoCollector().GetDetails()}\n", "SystemDetails", false);
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Error, $"Failed to collect system info: {e.Message}", "SystemDetails");
            }
        }



        private static void RunBenchmark()
        {
            Log.Print("Starting benchmark...");

            TimeSpan span;

            if (RunOptions.RunPrime)
            {
                Log.Print("Starting single thread performance tests...");

                span = Benchmark.SingleThreadPrimeBenchmark(Tests.small_start, Tests.small_max);
                Log.LogEvent(Log.EventType.Performance, $"Single thread (small): {span.TotalMilliseconds:N4}", "Benchmark", false);
                Log.Print($"Single small values: {span.TotalMilliseconds:N4} ms");

                span = Benchmark.SingleThreadPrimeBenchmark(Tests.med_start, Tests.med_max);
                Log.LogEvent(Log.EventType.Performance, $"Single thread (med): {span.TotalMilliseconds:N4}", "Benchmark", false);
                Log.Print($"Single medium values: {span.TotalMilliseconds:N4} ms");

                span = Benchmark.SingleThreadPrimeBenchmark(Tests.large_start, Tests.large_max);
                Log.LogEvent(Log.EventType.Performance, $"Single thread (large): {span.TotalMilliseconds:N4}", "Benchmark", false);
                Log.Print($"Single large values: {span.TotalMilliseconds:N4} ms");

                //span = Benchmark.SingleThreadPrimeBenchmark(Tests.huge_start, Tests.huge_max);
                //Log.LogEvent(Log.EventType.Performance, $"Single thread (huge): {span.TotalMilliseconds:N4}\n", "Benchmark", false);
                //Log.Print($"Single huge values: {span.TotalMilliseconds:N4} ms");

                Log.Print("Single threads tests complete.");
                Log.Print("Starting multi thread performance tests...");

                span = Benchmark.MultiThreadPrimeBenchmark(Tests.small_start, Tests.small_max, Environment.ProcessorCount);
                Log.LogEvent(Log.EventType.Performance, $"Multi thread (small): {span.TotalMilliseconds:N4}", "Benchmark", false);
                Log.Print($"Multi small values: {span.TotalMilliseconds:N4} ms");

                span = Benchmark.MultiThreadPrimeBenchmark(Tests.med_start, Tests.med_max, Environment.ProcessorCount);
                Log.LogEvent(Log.EventType.Performance, $"Multi thread (medium): {span.TotalMilliseconds:N4}", "Benchmark", false);
                Log.Print($"Multi medium values: {span.TotalMilliseconds:N4} ms");

                span = Benchmark.MultiThreadPrimeBenchmark(Tests.large_start, Tests.large_max, Environment.ProcessorCount);
                Log.LogEvent(Log.EventType.Performance, $"Multi thread (large): {span.TotalMilliseconds:N4}", "Benchmark", false);
                Log.Print($"Multi large values: {span.TotalMilliseconds:N4} ms");

                span = Benchmark.MultiThreadPrimeBenchmark(Tests.huge_start, Tests.huge_max, Environment.ProcessorCount);
                Log.LogEvent(Log.EventType.Performance, $"Multi thread (huge): {span.TotalMilliseconds:N4}", "Benchmark", false);
                Log.Print($"Multi huge values: {span.TotalMilliseconds:N4} ms");

                Log.Print("Multi threads tests complete.");
            }
            
            if (RunOptions.RunSqrt)
            {
                throw new NotImplementedException();
            }

            Log.Print("Tests complete.");
        }
    }
}
