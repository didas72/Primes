using System;
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
