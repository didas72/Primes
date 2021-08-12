using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes.Exec
{
    public class JobDistributer
    {
        private volatile bool distribute = false;
        private readonly string jobPath;
        private Thread distributingThread;
        public Worker[] Workers { get; private set; }



        public JobDistributer(ushort workerCount, string jobPath, string dumpPath)
        {
            this.jobPath = jobPath;

            Workers = new Worker[workerCount];

            for (int i = 0; i < workerCount; i++)
                Workers[i] = new Worker(dumpPath, jobPath, i) { primeBufferSize = Properties.Settings.Default.PrimeBufferSize };
        }



        public void StartWork()
        {
            distribute = true;

            distributingThread = new Thread(() => DistributingLoop());
            distributingThread.Start();
        }
        public void StopWork()
        {
            distribute = false;

            StopAllWorkers();
        }



        private void DistributingLoop()
        {
            Program.LogEvent(Program.EventType.Info, "Loading jobs.", "DistributingThread", true);

            Queue<string> jobFiles = Utils.GetDoableJobs(jobPath, Properties.Settings.Default.MaxJobQueue);

            Program.LogEvent(Program.EventType.Info, "Jobs loaded.", "DistributingThread", true);
            Program.LogEvent(Program.EventType.Info, "Work started.", "DistributingThread", true);

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
                            Program.LogEvent(Program.EventType.Warning, $"Failed to deserialize job from file '{path}' for computing. Skipping. {e.Message}", "DistributingThread", true);
                            continue;
                        }
                    }
                    else if (jobFiles.Count <= 0)
                    {
                        jobFiles = Utils.GetDoableJobs(jobPath, Properties.Settings.Default.MaxJobQueue);

                        if (jobFiles.Count <= 0)
                        {
                            Program.LogEvent(Program.EventType.Info, "Finished distributing all jobs.", "JobDistributingThread", true);

                            distribute = false;

                            break;
                        }
                    }
                }

                Thread.Sleep(50);
            }

            WaitForAllWorkers();
            Program.Exit(true);
        }



        private void StopAllWorkers()
        {
            Program.LogEvent(Program.EventType.Info, "Stopping Workers.", "MainThread", true);

            for (int i = 0; i < Workers.Length; i++)
            {
                Workers[i].StopWork();
            }
        }
        public void WaitForAllWorkers()
        {
            bool anyWorking = true;

            while(anyWorking)
            {
                Thread.Sleep(100);

                anyWorking = false;

                for (int i = 0; i < Workers.Length; i++)
                {
                    if (Workers[i].IsWorking)
                    {
                        anyWorking = true;
                    }
                }
            }
        }
    }
}
