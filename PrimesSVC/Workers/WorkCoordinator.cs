using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Extensions;

using Primes.Common;
using Primes.Common.Files;

namespace Primes.SVC
{
    internal static class WorkCoordinator
    {
        private static readonly Semaphore jobQueueAccess = new(0, 1);
        private static Queue<string> jobQueue = new();
        private static int maxJobQueue = -1;

        private static Worker[] workers;



        public static bool InitWorkers()
        {
            workers = new Worker[Settings.Threads];
            int bufferSize = Settings.PrimeBufferSize;

            for (int i = 0; i < workers.Length; i++)
                workers[i] = new Worker(bufferSize);

            return true;
        }


        public static void StartWork()
        {
            for (int i = 0; i < workers.Length; i++)
                workers[i].Start();
        }
        public static void StopWork()
        {
            if (workers == null) return;

            for (int i = 0; i < workers.Length; i++)
                workers[i]?.Stop();
        }
        public static void WaitForWorkers(TimeSpan maxWait)
        {
            if (workers == null || workers.Length == 0) return;

            Stopwatch sw = new();
            sw.Start();
            int i = 0;

            while (sw.Elapsed < maxWait)
            {
                if (!workers[i].IsRunning())
                    i++;

                if (i >= workers.Length) break;

                Thread.Sleep(1);
            }

            sw.Stop();
        }



        public static PrimeJob GetNextPrimeJob()
        {
            jobQueueAccess.WaitOne();

            if (jobQueue.Count <= 0)
                EnqueueJobs();

            string path = jobQueue.Dequeue();

            jobQueueAccess.Release();

            if (jobQueue.Count <= 0)
                return null;

            return PrimeJob.Deserialize(path);
        }
        private static void EnqueueJobs()
        {
            if (maxJobQueue == -1)
                maxJobQueue = Settings.GetMaxJobQueue();

            jobQueue = PrimesUtils.GetDoableJobs(Globals.jobsDir, (uint)maxJobQueue, true);
        }
    }
}
