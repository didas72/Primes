using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Primes.SVC
{
    internal static class Scheduler
    {
        private static Timer batchGetRetry, batchReturn;
        public static ElapsedEventHandler OnBatchGetRetry;
        public static ElapsedEventHandler OnBatchReturn;

        public static bool Init()
        {
            batchGetRetry = new() { AutoReset = true, Interval = 1000 * 60 * 30 }; //every 30 mins
            batchReturn = new() { AutoReset = true, Interval = 1000 * 60 * 60 * 2 }; //2 hours

            batchGetRetry.Elapsed += OnBatchGetRetry;
            batchReturn.Elapsed += OnBatchReturn;

            batchReturn.Start();

            return true;
        }



        public static void EnableBatchGetRetry() => batchGetRetry.Start();
        public static void DisableBatchGetRetry() => batchGetRetry.Stop();
    }
}
