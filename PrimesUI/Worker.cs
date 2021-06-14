using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Primes.Common;
using Primes.Common.Files;

namespace PrimesUI
{
    class Worker
    {
        public bool IsWorking { get {
            if (Thread == null) return false;
            return Thread.IsAlive;
        }}
        public uint CurrentBatch { get; private set; }
        public Thread Thread { get; private set; }
        public float Progress { get; private set; }
        public int WorkerID { get; private set; }



        public EventHandler<WorkerStoppedEventArgs> OnStop { get; set; }



        private volatile bool doWork = false;
        private Distributer dist;
        private readonly int primeBufferSize; 



        public Worker(int workerID, int primeBufferSize)
        {
            Thread = new Thread(DoWork);
            Progress = 0f;

            WorkerID = workerID;
            this.primeBufferSize = primeBufferSize;
        }



        public void StartWork(ref Distributer dist)
        {
            this.dist = dist;

            doWork = true;

            Thread.Start();
        }

        public void StopWork()
        {
            doWork = false;
        }

        public void StopAndJoin()
        {
            StopWork();

            Thread.Join();
        }



        private void DoWork()
        {
            while (doWork)
            {
                if (!dist.GetPendingPrimeJob(out PrimeJob job))
                {
                    doWork = false;

                    OnStop(this, new WorkerStoppedEventArgs(PrimeJob.Empty, WorkerStoppedEventArgs.StopReason.NoJob));

                    return;
                }



                CurrentBatch = job.Batch;

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

                        break;
                    }



                    if (PrimesProgram.resourcesLoaded)
                        result = Mathf.IsPrime(current, ref PrimesProgram.knownPrimes);
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
                        {
                            OnStop(this, new WorkerStoppedEventArgs(job, WorkerStoppedEventArgs.StopReason.Paused));

                            return;
                        }
                    }

                    current += 2;
                }



                CurrentBatch = 0;
                Progress = 0f;
            }
        }
    }

    public class WorkerStoppedEventArgs : EventArgs
    {
        public PrimeJob job;
        public StopReason reason;

        public WorkerStoppedEventArgs(PrimeJob job, StopReason reason)
        {
            this.job = job; this.reason = reason;
        }

        public enum StopReason : byte
        {
            Finished,
            Paused,
            NoJob
        }
    }
}
