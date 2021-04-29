using System;
using System.IO;
using System.Threading;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes
{
    public class Worker
    {
        public Thread Thread { get; private set; }
        public float Progress { get; private set; }
        public uint CurrentBatch { get; private set; } = 0;
        public bool IsWorking { get {
                if (Thread == null) return false;
                else return Thread.IsAlive;
            }  }
        private volatile bool doWork = false;
        private readonly string dumpPath;
        private readonly string jobPath;
        private readonly int workerId;
        private const int primeBufferSize = 500;



        public Worker(string dumpPath, string jobPath, int workerId)
        {
            this.dumpPath = dumpPath; this.jobPath = jobPath; this.workerId = workerId;
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



        public void DoWork(PrimeJob job)
        {
            CurrentBatch = job.batch;

            DateTime startingTime = DateTime.Now;

            ulong current = Math.Max(job.start + job.progress, 2);
            bool result;

            if (current == 2) { job.primes.Add(2); current++; } //2 exception if we're there
            else if (current % 2 == 0) current++; //ignore if pair

            ulong[] primes = new ulong[primeBufferSize];//temp buffer for primes
            int i = 0;



            while (true)
            {
                if (current >= job.start + job.count)
                {
                    job.progress = job.count;

                    TimeSpan elapsed = DateTime.Now - startingTime;

                    Program.LogEvent(Program.EventType.Info, $"Finished job {job.start} of batch {job.batch}. Elapsed {elapsed.Hours}:{elapsed.Minutes}:{elapsed.Seconds}. Saving.", $"WorkerThread#{workerId:D2}", true);

                    ConsoleUI.RegisterJobSeconds(workerId, elapsed.TotalSeconds);

                    break;
                }



                if (Program.resourcesLoaded)
                    result = Mathf.IsPrime(current, ref Program.knowPrimes);
                else
                    result = Mathf.IsPrime(current);



                if (result)
                {
                    primes[i++] = current;

                    Progress = (float)((current - job.start) * 100 / (double)job.count);

                    if (i >= primes.Length)
                    {
                        job.primes.AddRange(primes);//only add the primes to the list when we have 100 (avoid redoing arrays hundreds of times)
                        i = 0;
                        primes = new ulong[primeBufferSize];
                    }

                    if (!doWork)
                    {
                        Program.LogEvent(Program.EventType.Info, $"Pausing job {job.start} of batch {job.batch}. Saving.", $"WorkerThread#{workerId:D2}", true);

                        break;
                    }
                }

                current += 2;
            }



            job.primes.AddRange(GetFirst(primes, i));



            if (job.progress != job.count)
            {
                job.progress = current - job.start + 1;

                try
                {
                    job.Serialize(Path.Combine(jobPath, $"{job.start}.primejob"));
                }
                catch (Exception e)
                {
                    Program.LogEvent(Program.EventType.Error, $"Failed to serialize prime job {jobPath}. {e.Message}", $"WorkerThread#{workerId:D2}", true);
                }
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(dumpPath, $"{job.batch}"));

                try
                {
                    job.Serialize(Path.Combine(dumpPath, $"{job.batch}\\{job.start}.primejob"));
                }
                catch (Exception e)
                {
                    Program.LogEvent(Program.EventType.Error, $"Failed to serialize prime job {jobPath}. {e.Message}", $"WorkerThread#{workerId:D2}", true);
                }
            }



            doWork = false;

            CurrentBatch = 0;
        }
        private ulong[] GetFirst(ulong[] nums, int amount)
        {
            ulong[] ret = new ulong[amount];

            for (int i = 0; i < amount; i++)
                ret[i] = nums[i];

            return ret;
        }
    }
}
