using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using Primes;
using Primes.Common;

namespace Primes.Tester
{
    static class Benchmark
    {
        public static TimeSpan SingleThreadPrimeBenchmark(ulong start, ulong max)
        {
            bool last = false;

            Stopwatch s = new Stopwatch();

            s.Start();

            for (ulong i = start; i < max; i++)
            {
                if (!last)
                    last = Mathf.IsPrime(i);
            }

            s.Stop();

            return s.Elapsed;
        }
    }
}
