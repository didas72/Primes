using System;
using System.Diagnostics;
using System.Threading;

using Primes.Common;

namespace Primes.Tester
{
    public static class Benchmark
    {
        public static TimeSpan SingleThreadPrimeBenchmark(ulong start, ulong max)
        {
            bool last = false;

            Stopwatch s = new Stopwatch();

            s.Start();

            for (ulong i = start; i < max; i++)
            {
                if (!last)
                    last = PrimesMath.IsPrime(i);
            }

            s.Stop();

            return s.Elapsed;
        }

        public static TimeSpan MultiThreadPrimeBenchmark(ulong start, ulong max, int threadCount)
        {
            if (threadCount < 2)
                throw new ArgumentException();

            Thread[] threads = new Thread[threadCount];
            TimeSpan[] spans = new TimeSpan[threadCount];

            for (int i = 0; i < threads.Length; i++)
            {
                Console.WriteLine($"{i}:{threads.Length}:{spans.Length}");
                spans[i] = new TimeSpan();
                threads[i] = new Thread(() => MultiThreadRun(start, max, ref spans[i]));
            }

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Start();
            }

            for (int i = 0; i < threads.Length; i++)
            {
                threads[i].Join();
            }

            long totalTicks = 0;
            for (int i = 0; i < spans.Length; i++)
            {
                totalTicks += spans[i].Ticks;
            }
            
            totalTicks /= threadCount;

            return new TimeSpan(totalTicks);
        }

        private static void MultiThreadRun(ulong start, ulong max, ref TimeSpan span)
        {
            bool last = false;

            Stopwatch s = new Stopwatch();

            s.Start();

            for (ulong i = start; i < max; i++)
            {
                if (!last)
                    last = PrimesMath.IsPrime(i);
            }

            s.Stop();

            span = s.Elapsed;
        }
    }
}
