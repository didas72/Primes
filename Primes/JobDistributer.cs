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
            try
            {
                Queue<string> jobFiles;

                jobFiles = TryGetSorted();

                LogExtension.LogEvent(Log.EventType.Info, "Work started.", "DistributingThread", true);

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
                                LogExtension.LogEvent(Log.EventType.Warning, $"Failed to deserialize job from file '{path}' for computing. Skipping.", "DistributingThread", true, false);
                                Log.LogException($"Failed to deserialize job from file '{path}' for computing.", "DistributingThread", e);
                                continue;
                            }
                        }
                        else if (jobFiles.Count <= 0)
                        {
                            jobFiles = TryGetSorted();

                            if (jobFiles.Count <= 0)
                            {
                                LogExtension.LogEvent(Log.EventType.Info, "Finished distributing all jobs.", "JobDistributingThread", true);

                                distribute = false;

                                break;
                            }
                        }
                    }

                    Thread.Sleep(50);
                }

                WaitForAllWorkers();
            }
            catch (Exception e)
            {
                LogExtension.LogEvent(Log.EventType.Error, "JobDistributer crashed.", "JobDistributer", true, false);
                Log.LogException("JobDistributer crashed.", "JobDistributer", e);

                try
                {
                    StopAllWorkers();
                    WaitForAllWorkers();
                }
                catch { }
            }
            
            Program.Exit(true);
        }



        private Queue<string> TryGetSorted()
        {
            Log.LogEvent($"Loading {Properties.Settings.Default.MaxJobQueue} jobs for processing.", "DistributingThread");

            try
            {
                return Utils.GetDoableJobs(jobPath, Properties.Settings.Default.MaxJobQueue, true);
            }
            catch (Exception e)
            {
                LogExtension.LogEvent(Log.EventType.Warning, "Failed to sort files for execution.", "DistributingThread", true, false);
                Log.LogException("Failed to sort files for execution.", "DistributingThread", e);

                return Utils.GetDoableJobs(jobPath, Properties.Settings.Default.MaxJobQueue, false);
            }
        }




        private void StopAllWorkers()
        {
            LogExtension.LogEvent(Log.EventType.Info, "Stopping Workers.", "MainThread", true);

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
