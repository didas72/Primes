using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Primes.Tester
{
    static class RunOptions
    {
        public static bool RunBenchmark = false;
        public static bool RunStressTest = false;
        public static bool RunSqrt = false;
        public static bool RunPrime = false;
        public static int Threads = Environment.ProcessorCount;
        public static bool CollectSysInfo = true;

        public static string ExportOptions()
        {
            return $"{RunBenchmark};{RunStressTest};{RunSqrt};{RunPrime};{Threads};{CollectSysInfo}\n";
        }
    }
}
