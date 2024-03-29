﻿using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using DidasUtils.Logging;
using DidasUtils.Extensions;

using Primes.Common;
using Primes.Common.Files;

namespace Primes.Exec
{
    public class Worker
    {
        public Thread WThread { get; protected set; }
        public float Progress { get; protected set; }
        public uint CurrentBatch { get; protected set; } = 0;
        public bool IsWorking { get {
                if (WThread == null) return false;
                else return WThread.IsAlive;
            }  }
        protected volatile bool doWork = false;
        protected readonly string dumpPath;
        protected readonly string jobPath;
        protected readonly int workerId;
        protected uint updatesFrame;
        public uint primeBufferSize = 500;
        public uint maxNoUpdates = 10000;



        public Worker(string dumpPath, string jobPath, int workerId)
        {
            this.dumpPath = dumpPath; this.jobPath = jobPath; this.workerId = workerId;
        }



        public virtual void StartWork(PrimeJob job)
        {
            doWork = true;
            
            WThread = new Thread(() => DoWork(job));
            WThread.Start();
        }
        public virtual void StopWork()
        {
            doWork = false;
        }



        public virtual void DoWork(PrimeJob job)
        {
            try
            {
                updatesFrame = 0;
                CurrentBatch = job.Batch;
                DateTime startingTime = DateTime.Now;

                ulong current = Math.Max(job.Start + job.Progress, 2);
                ulong maxToCheck = job.Start + job.Count;
                bool result;
                int i = 0;

                if (current == 2) { job.Primes.Add(2); current++; } //2 exception if we're there
                else if (current % 2 == 0) current++; //ignore if even

                ulong[] primes = new ulong[primeBufferSize];//temp buffer for primes



                while (true)
                {
                    if (current >= maxToCheck)
                    {
                        job.Progress = job.Count;
                        break;
                    }



                    if (Program.resourcesLoaded)
                        result = PrimesMath.IsPrime(current, Program.knowPrimes);
                    else
                        result = PrimesMath.IsPrime(current);



                    if (result)
                    {
                        primes[i++] = current;

                        if (i >= primes.Length)
                        {
                            job.Primes.AddRange(primes);//only add the primes to the list when we have primeBufferSize (avoid redoing arrays hundreds of times)
                            i = 0;
                            primes = new ulong[primeBufferSize];
                        }
                    }



                    current += 2;
                    updatesFrame++;

                    //don't check every prime found/tested
                    if (updatesFrame >= maxNoUpdates)
                    {
                        updatesFrame = 0;

                        Progress = (float)((current - job.Start) * 100 / (double)job.Count);

                        if (!doWork)
                        {
                            LogExtension.LogEvent(Log.EventType.Info, $"Pausing job {job.Start} of batch {job.Batch}. Saving.", $"WThread#{workerId:D2}", true);
                            break;
                        }
                    }
                }



                job.Primes.AddRange(primes.GetFirst(i));



                SaveJob(job, current);



                TimeSpan elapsed = DateTime.Now - startingTime;

                if (doWork)
                    LogExtension.LogEvent(Log.EventType.Info, $"Finished job {job.Start} of batch {job.Batch}. Elapsed {elapsed.Hours}:{elapsed.Minutes}:{elapsed.Seconds}. Saving.", $"WThread#{workerId:D2}", true);

                if (ConsoleUI.UIEnabled)
                    ConsoleUI.RegisterJobSeconds(workerId, elapsed.TotalSeconds);
            }
            catch(Exception e)
            {
                Log.LogException($"Worker#{workerId} crashed.", $"Worker#{workerId:D2}", e);
                LogExtension.LogEvent(Log.EventType.Error, "A worker has crashed. Check log for details.", $"Worker#{workerId:D2}", true, false);

                SaveJob_Crash(job);
            }

            doWork = false;

            CurrentBatch = 0;
        }



        protected virtual void SaveJob(PrimeJob job, ulong current)
        {
            if (job.Progress != job.Count)
            {
                job.Progress = current - job.Start;

                SavePausedJob(job);
            }
            else
            {
                SaveFinishedJob(job);
            }
        }
        protected virtual void SaveFinishedJob(PrimeJob job)
        {
            try
            {
                Directory.CreateDirectory(Path.Combine(dumpPath, $"{job.Batch}"));
                string filePath = Path.Combine(dumpPath, $"{job.Batch}\\{job.Start}.primejob");

                if (File.Exists(filePath))
                    LogExtension.LogEvent(Log.EventType.Warning, $"Completed job {job.Start}.primejob already exists. Overwritting.", $"Worker#{workerId:D2}");

                PrimeJob.Serialize(job, filePath);
            }
            catch (Exception e)
            {
                LogExtension.LogEvent(Log.EventType.Error, "Failed to serialize finished primejob.", $"WThread#{workerId:D2}", true, false);
                Log.LogException("Failed to serialize finished primejob.", $"WThread#{workerId:D2}", e);

                SaveJob_Crash(job);
            }
        }
        protected virtual void SavePausedJob(PrimeJob job)
        {
            try
            {
                PrimeJob.Serialize(job, Path.Combine(jobPath, $"{job.Start}.primejob"));
            }
            catch (Exception e)
            {
                LogExtension.LogEvent(Log.EventType.Error, "Failed to serialize paused primejob.", $"WThread#{workerId:D2}", true, false);
                Log.LogException("Failed to serialize paused primejob.", $"WThread#{workerId:D2}", e);

                SaveJob_Crash(job);
            }
        }
        protected virtual void SaveJob_Crash(PrimeJob job)
        {
            try
            {
                PrimeJob save = new(job.FileVersion, job.FileCompression, job.Batch, job.Start, job.Count, 0, new List<ulong>());
                PrimeJob.Serialize(save, Path.Combine(jobPath, $"{job.Start}.FAILED"));
            }
            catch (Exception e)
            {
                LogExtension.LogEvent(Log.EventType.Error, "Failed to restore job.", $"Worker#{workerId:D2}", true, false);
                Log.LogException("Failed to restore job.", $"Worker#{workerId:D2}", e);
            }
        }
    }
}
