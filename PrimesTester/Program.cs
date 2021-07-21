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
        private static Log log;

        static void Main(string[] args)
        {
            Console.WriteLine("PrimesTexter.exe by Didas72 and PeakRead");

            ParseArguments(ref args);

            log = new Log(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "PrimesTester.log.txt"));
            log.Write(Log.EventType.Info, "Start");
            log.WriteRaw($"Options: \n{RunOptions.ExportOptions()}\n");

            CollectSystemInfo();

            RunBenchmark();
        }

        private static void ParseArguments(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "/?":

                        Console.WriteLine("Simple benchmarking / stress testing tool based on prime checking.");
                        Console.WriteLine();
                        Console.WriteLine("Arguments:");
                        Console.WriteLine("/b          - Runs benchmark.");
                        Console.WriteLine("/b-prime    - Runs prime testing performance tests. Used by default. (use with /b)");
                        Console.WriteLine("/b-sqrt     - Runs square root performance tests. Not used by default. (use with /b)");
                        Console.WriteLine("/s          - Runs stress test. (Not implemented)");
                        Console.WriteLine("/threads-XX - Runs with XX threads. If not included the program will decide the most appropriate value.");
                        Console.WriteLine("/no-sys-info  - Disables collection of system information. Not set by default.");

                        Environment.Exit(0);

                        break;

                    case "/b":
                        RunOptions.RunBenchmark = true;
                        break;

                    case "/b-prime":
                        RunOptions.RunPrime = true;
                        break;

                    case "/b-sqrt":
                        RunOptions.RunSqrt = true;
                        break;

                    case "/s":
                        RunOptions.RunStressTest = true;
                        break;

                    case "/no-sys-info":
                        RunOptions.CollectSysInfo = false;
                        break;

                    default:

                        if (args[i].StartsWith("/threads-"))
                            if (!int.TryParse(args[i].Substring(9), out RunOptions.Threads))
                            {
                                Console.WriteLine("Invalid thread count!");
                                Thread.Sleep(2000);
                                Environment.Exit(0);
                            }
                        
                        break;
                }
            }
        }

        private static void CollectSystemInfo()
        {
            try
            {
                if (RunOptions.CollectSysInfo)
                {
                    Console.WriteLine("Collecting system details...");
                    log.WriteRaw($"System Info:\n{new InfoCollector().GetDetails()}\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to collect system info: {e.Message}");
            }
        }

        private static void RunBenchmark()
        {
            Console.WriteLine("Starting benchmark...");

            Console.WriteLine("Starting single thread performance tests...");

            TimeSpan span;

            if (RunOptions.RunPrime)
            {
                span = Benchmark.SingleThreadPrimeBenchmark(Tests.small_start, Tests.small_max);
                log.Write(Log.EventType.Performance, $"Single thread (small): {span.TotalMilliseconds:N4}");
                Console.WriteLine($"Small values: {span.TotalMilliseconds:N4} ms");

                span = Benchmark.SingleThreadPrimeBenchmark(Tests.med_start, Tests.med_max);
                log.Write(Log.EventType.Performance, $"Single thread (med): {span.TotalMilliseconds:N4}");
                Console.WriteLine($"Medium values: {span.TotalMilliseconds:N4} ms");

                span = Benchmark.SingleThreadPrimeBenchmark(Tests.large_start, Tests.large_max);
                log.Write(Log.EventType.Performance, $"Single thread (large): {span.TotalMilliseconds:N4}\n");
                Console.WriteLine($"Large values: {span.TotalMilliseconds:N4} ms");

                span = Benchmark.SingleThreadPrimeBenchmark(Tests.huge_start, Tests.huge_max);
                log.Write(Log.EventType.Performance, $"Single thread (huge): {span.TotalMilliseconds:N4}\n");
                Console.WriteLine($"Huge values: {span.TotalMilliseconds:N4} ms");
            }
            
            if (RunOptions.RunSqrt)
            {
                throw new NotImplementedException();
            }

            Console.WriteLine("Single threads tests complete.");
        }
    }
}
