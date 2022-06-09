using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.IO;
using System.Timers;

using DidasUtils;
using DidasUtils.Files;
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



        public static bool Init()
        {
            Scheduler.OnBatchReturn += OnBatchReturn;
            Scheduler.OnBatchGetRetry += OnBatchGetRetry;
            return true;
        }
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
                if (workers[i] == null || workers[i].IsRunning())
                    workers[i].Stop();
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



        public static bool IsWorkRunning()
        {
            if (workers == null || workers.Length == 0) return false;
            return workers.Any((Worker w) => (w != null && w.IsRunning()));
        }
        public static uint GetCurrentBatchNumber()
        {
            if (jobQueue == null || jobQueue.Count == 0) return 0;

            jobQueueAccess.WaitOne();
            string path = jobQueue.Peek();
            jobQueueAccess.Release();

            return PrimeJob.Deserialize(path).Batch;
        }
        public static float GetCurrentBatchProgress()
        {
            if (jobQueue == null || jobQueue.Count == 0) return 0;

            jobQueueAccess.WaitOne();
            string path = jobQueue.Peek();
            jobQueueAccess.Release();

            string dir = Path.GetDirectoryName(path);
            int left = Directory.GetFiles(dir, "*.primejob").Length;
            return left / 1000f;
        }



        public static PrimeJob GetNextPrimeJob(Semaphore stopCheck)
        {
            string path;

            start:

            if (!jobQueueAccess.WaitOne(500))
            {
                if (stopCheck.WaitOne(0))
                    return null;

                goto start;
            }

            if (jobQueue.Count <= 0)
                EnqueueJobs();

            if (jobQueue.Count <= 0)
                path = null;
            else
                path = jobQueue.Dequeue();

            jobQueueAccess.Release();

            if (path == null)
                return null;

            return PrimeJob.Deserialize(path);
        }
        private static void EnqueueJobs()
        {
            if (maxJobQueue == -1)
                maxJobQueue = Settings.GetMaxJobQueue();

            jobQueue = PrimesUtils.GetDoableJobs(Globals.jobsDir, maxJobQueue == 0 ? uint.MaxValue : (uint)maxJobQueue, true);

            if (jobQueue.Count <= 0)//no more available offline, check with JobDistributer
            {
                if (!GetOnlineJobs())//no more are available online, stop work
                    StopWork();
            }
        }
        private static bool GetOnlineJobs()
        {
            Scheduler.DisableBatchGetRetry();//this will kill the timer, so there are never two running instances of this
            //plus when it is brought back the timer will have reset

            if (!BatchManager.IsServerAccessible())
            {
                Scheduler.EnableBatchGetRetry();
                return false;
            }

            switch (BatchManager.GetBatch(TimeSpan.FromSeconds(1)))
            {
                case BatchManager.GetBatchStatus.UnspecifiedError:
                    Scheduler.EnableBatchGetRetry();
                    return false;

                case BatchManager.GetBatchStatus.Success:
                    Scheduler.DisableBatchGetRetry(); //not needed anymore
                    return ExtractReceived();

                case BatchManager.GetBatchStatus.NoAvailableBatches:
                    Scheduler.EnableBatchGetRetry();
                    return false;

                case BatchManager.GetBatchStatus.LimitReached:
                    if (RegetBatches()) return true;
                    Scheduler.EnableBatchGetRetry();
                    return false;

                default:
                    return false;
            }
        }
        private static bool RegetBatches()
        {
            Log.LogEvent(Log.EventType.Warning, "Local batches differs from online assignments. Resetting to online.", "RegetBatches");

            StopWork();
            Directory.Delete(Globals.jobsDir, true); Directory.CreateDirectory(Globals.jobsDir);
            Directory.Delete(Globals.completeDir, true); Directory.CreateDirectory(Globals.completeDir);

            BatchManager.RegetBatchesStatus status = BatchManager.RegetAllBatches(TimeSpan.FromSeconds(1));

            switch (status)
            {
                case BatchManager.RegetBatchesStatus.Success:
                    if (ExtractReceived())
                    {
                        StartWork();
                        return true;
                    }
                    else
                        return false;

                case BatchManager.RegetBatchesStatus.NoBatchesAssigned:
                    return false; //no need to extract, resume normal operation

                case BatchManager.RegetBatchesStatus.InvalidId:
                    ResetLocal();
                    return false;

                case BatchManager.RegetBatchesStatus.UnspecifiedError:
                    Log.LogEvent(Log.EventType.HighWarning, "TryRegetBatches failed with an unspecified error. Stopping work and rescheduling restart.", "TryRegetBatches");
                    return false; //back up the call stack, we end up either in OnBatchGetRetry or EnqueueJobs. Either way, work is stopped (or was previously) and Scheduler is activated (or stays active)
            }

            //if we somehow get here
            return false;
        }
        

        
        private static void OnBatchReturn(object sender, ElapsedEventArgs args)
        {
            //hopefully I'll stick to the 1 batch = 1k jobs format bc it kinda depends on that, also the batch will be lost if a single job is lost

            if (!BatchManager.IsServerAccessible()) return;

            try
            {
                string[] completeBatches = Directory.GetDirectories(Globals.completeDir);

                foreach (string dir in completeBatches)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(dir, "*.primejob", SearchOption.TopDirectoryOnly);
                        if (files.Length != 1000) continue;
                        bool goodBatch = true;

                        foreach (string f in files)
                        {
                            if (PrimeJob.PeekStatusFromFile(f) != PrimeJob.Status.Finished)
                            {
                                goodBatch = false;
                                break;
                            }
                        }

                        if (goodBatch)
                        {
                            BatchManager.ReturnBatchStatus status = BatchManager.ReturnBatch(dir, TimeSpan.FromSeconds(1));

                            switch (status)
                            {
                                case BatchManager.ReturnBatchStatus.Success:
                                    Directory.Delete(dir, true);
                                    Log.LogEvent($"Successfully returned batch '{Path.GetFileName(dir)}'.", "OnBatchReturn");
                                    break;

                                case BatchManager.ReturnBatchStatus.BatchNotAssigned:
                                    Log.LogEvent(Log.EventType.Warning, $"Batch {Path.GetFileName(dir)} was not assigned. Deleting", "OnBatchReturn");
                                    Directory.Delete(dir, true);
                                    break;

                                case BatchManager.ReturnBatchStatus.InvalidId:
                                    Log.LogEvent(Log.EventType.Warning, "Current clientId is invalid. Deleting files.", "OnBatchReturn");
                                    ResetLocal();
                                    return; //no need to do anything else since all batches are gone

                                case BatchManager.ReturnBatchStatus.CouldNotDetermineBatchNum:
                                    Log.LogEvent(Log.EventType.Warning, $"Could not determine batchNum for batch {Path.GetFileName(dir)}. Deleting", "OnBatchReturn");
                                    Directory.Delete(dir, true);
                                    break;

                                case BatchManager.ReturnBatchStatus.UnspecifiedError:
                                    Log.LogEvent(Log.EventType.Warning, $"Failed to return batch '{Path.GetFileName(dir)}'. Deleting.", "OnBatchReturn");
                                    Directory.Delete(dir, true);
                                    break;

                            }
                        }
                    }
                    catch { }
                }
            } 
            catch (Exception e)
            {
                Log.LogException("Failed to return batches.", "OnBatchReturn", e);
            }
        }
        private static void OnBatchGetRetry(object sender, ElapsedEventArgs e)
        {
            //Actually not needed since work should be stopped
            //but to be sure stop work anyway (don't want other threads messing with this)
            StopWork();

            if (GetOnlineJobs())
            {
                Scheduler.DisableBatchGetRetry();
                StartWork();
            }
            else
                return; //no can do
        }



        private static void ResetLocal()
        {
            StopWork();
            Directory.Delete(Globals.jobsDir, true); Directory.CreateDirectory(Globals.jobsDir);
            Directory.Delete(Globals.completeDir, true); Directory.CreateDirectory(Globals.completeDir);
            StartWork();
        }
        private static bool ExtractReceived()
        {
            string archivePath = Path.Combine(Globals.cacheDir, "batchDownload.7z.tmp");

            if (!SevenZip.TryDecompress7z(archivePath, Globals.cacheDir))
            {
                Log.LogEvent(Log.EventType.Warning, "Failed to extract received batch.", "GetOnlineJobs");
                File.Delete(archivePath);
                return false;
            }

            string[] dirs = Directory.GetDirectories(Globals.cacheDir);
            foreach (string dir in dirs)
            {
                if (long.TryParse(dir, out _)) //assume it is what we want, since cache should be empty
                {
                    Utils.CopyDirectory(dir, Globals.jobsDir);
                    Directory.Delete(dir, true);
                }
            }

            return true;
        }
    }
}
