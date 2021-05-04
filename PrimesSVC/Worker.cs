using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes.Service
{
    class Worker
    {
        public Thread Thread { get; set; }
        public int WorkerID { get; }
        public bool IsWorking { get {
                if (Thread == null) return false;
                else return Thread.IsAlive;
            }  }
        public float Progress { get; private set; }
        public uint CurrentBatch { get; private set; } = 0;



        private volatile bool doWork = false;
        private const int primeBufferSize = 500;



        public EventHandler<JobCompleteArgs> JobComplete;



        public Worker(int workerID)
        {
            WorkerID = workerID;
        }



        public void StartWork(PrimeJob job)
        {
            doWork = true;

            Thread = new Thread(() => DoWork(job));
            Thread.Start();
        }
        public void StopWork()
        {
            doWork = false;
        }



        private void DoWork(PrimeJob job)
        {
            CurrentBatch = job.Batch;

            DateTime startingTime = DateTime.Now;

            ulong current = Math.Max(job.Start + job.Progress, 2);
            bool result;

            if (current == 2) { job.Primes.Add(2); current++; } //2 exception if we're there
            else if (current % 2 == 0) current++; //ignore if pair

            ulong[] primes = new ulong[primeBufferSize];//temp buffer for primes
            int i = 0;



            while (true)
            {
                if (current >= job.Start + job.Count)
                {
                    Progress = job.Count;

                    job.Primes.AddRange(primes.GetFirst(i));

                    break;
                }



                if (Program.knowPrimes != null)
                    result = Mathf.IsPrime(current, ref Program.knowPrimes);
                else
                    result = Mathf.IsPrime(current);



                if (result)
                {
                    primes[i++] = current;

                    Progress = (float)((current - job.Start) * 100 / (double)job.Count);

                    if (i >= primes.Length)
                    {
                        job.Primes.AddRange(primes);//only add the primes to the list when we have 100 (avoid redoing arrays hundreds of times)
                        i = 0;
                        primes = new ulong[primeBufferSize];
                    }

                    if (!doWork)
                        break;
                }

                current += 2;
            }



            job.Progress = current - job.Start + 1;

            //Instead of serializing the job, leave it for someone else



            doWork = false;
            CurrentBatch = 0;

            JobComplete(this, new JobCompleteArgs() { Job = job, Elapsed = DateTime.Now - startingTime });
        }
    }



    public class JobCompleteArgs : EventArgs
    {
        public PrimeJob Job { get; set; }
        public TimeSpan Elapsed { get; set; }
    }
}
