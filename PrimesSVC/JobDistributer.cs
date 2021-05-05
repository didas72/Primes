using System;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes.Service
{
    public class JobDistributer
    {
        public Worker[] Workers { get; private set; }
        public List<PrimeJob> PendingJobDumps { get; set; }
        public List<TimeSpan> LastElapsed { get; set; }
        private Thread distributingThread;
        private Thread dumpingThread;
        private volatile bool distribute = false;
        private readonly string jobPath;
        private readonly string completePath;



        public JobDistributer(string jobPath, string completePath, int workerCount)
        {
            this.jobPath = jobPath;
            this.completePath = completePath;

            PendingJobDumps = new List<PrimeJob>();
            LastElapsed = new List<TimeSpan>();

            Workers = new Worker[workerCount];

            for (int i = 0; i < workerCount; i++)
            {
                Workers[i] = new Worker(i);
                Workers[i].JobComplete += JobComplete;
            }
        }



        public void StartWork()
        {
            if (!distribute)
            {
                distribute = true;

                distributingThread = new Thread(() => DistributingLoop());
                distributingThread.Start();

                dumpingThread = new Thread(DumpingLoop);
                dumpingThread.Start();
            }
        }
        public void StopWork()
        {
            distribute = false;

            StopAllWorkers();
            WaitForAllWorkers();
        }



        public void RescaleWorkers(int workerCount)
        {
            if (Working())
                StopWork();

            Workers = new Worker[workerCount];

            for (int i = 0; i < workerCount; i++)
            {
                Workers[i] = new Worker(i);
                Workers[i].JobComplete += JobComplete;
            }
        }



        private void DistributingLoop()
        {
            Queue<string> jobFiles = Utils.GetDoableJobs(jobPath);

            while (distribute)
            {
                for (int i = 0; i < Workers.Length; i++)
                {
                    if (!Workers[i].IsWorking && jobFiles.Count > 0)
                    {
                        string path = jobFiles.Dequeue();

                        try
                        {
                            PrimeJob job = PrimeJob.Deserialize(path);

                            File.Delete(path);

                            Workers[i].StartWork(job);
                        }
                        catch (Exception e)
                        {
                            PrimesProgram.log.WriteEntry($"{e.Message}", EventLogEntryType.Error);

                            continue;
                        }
                    }
                    else if (jobFiles.Count <= 0)
                    {
                        distribute = false;

                        break;
                    }
                }
            }

            WaitForAllWorkers();
        }
        private void DumpingLoop()
        {
            PrimeJob job;

            while (distribute || Working() || PendingJobDumps.Count != 0)
            {
                lock (PendingJobDumps)
                {
                    if (PendingJobDumps.Count == 0)
                    {
                        Thread.Sleep(50);

                        continue;
                    }

                    job = PendingJobDumps[0];
                    PendingJobDumps.RemoveAt(0);
                }

                switch (job.PeekStatus())
                {
                    case PrimeJob.Status.Finished:

                        Directory.CreateDirectory(Path.Combine(completePath, job.Batch.ToString()));

                        try
                        {
                            job.Serialize(Path.Combine(completePath, $"{job.Batch}\\{job.Start}.primejob"));
                        }
                        catch (Exception e)
                        {
                            PrimesProgram.log.WriteEntry($"{e.Message}", EventLogEntryType.Error);
                        }

                        break;

                    case PrimeJob.Status.Started:

                        try
                        {
                            job.Serialize(Path.Combine(jobPath, $"{job.Start}.primejob"));
                        }
                        catch (Exception e)
                        {
                            PrimesProgram.log.WriteEntry($"{e.Message}", EventLogEntryType.Error);
                        }

                        break;
                }

                job = null;
            }
        }



        private void StopAllWorkers()
        {
            for (int i = 0; i < Workers.Length; i++)
            {
                Workers[i].StopWork();
            }
        }
        public void WaitForAllWorkers()
        {
            while (Working()) Thread.Sleep(100);
        }



        public void JobComplete(object sender, JobCompleteArgs args)
        {
            lock (PendingJobDumps)
            {
                PendingJobDumps.Add(args.Job);
            }

            lock (LastElapsed)
            {
                LastElapsed.Add(args.Elapsed);
            }
        }
        


        public bool Working()
        {
            foreach (Worker w in Workers)
                if (w.IsWorking) return true;

            return false;
        }
    }
}
