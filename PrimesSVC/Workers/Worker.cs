using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.IO;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Extensions;

using Primes.Common;
using Primes.Common.Files;

namespace Primes.SVC
{
    internal class Worker
    {
        /// <summary>
        /// ONLY TO BE USED INSIDE WorkCoordinator
        /// </summary>
        public volatile bool shouldStop;

        private readonly ulong[] primeBuffer;
        private int bufferHead;
        private PrimeJob job;
        private bool isPrime;
        private ulong current, max;
        private readonly Stopwatch sw;
        private Thread selfThread;
        private int workerID;



        public Worker(int primeBufferSize, int workerID)
        {
            primeBuffer = new ulong[primeBufferSize];
            sw = new();
            job = null;
            selfThread = null;
            this.workerID = workerID;
        }



        public void Start()
        {
            shouldStop = false;

            if (selfThread == null || !selfThread.IsAlive)
                selfThread = new Thread(WorkLoop);

            selfThread.Start();
        }
        public void Stop()
        {
            shouldStop = true;
        }

        public bool IsRunning()
        {
            if (selfThread == null) return false;
            return selfThread.IsAlive;
        }



        private void WorkLoop()
        {
            string jobPath, jobRename;

            while (!shouldStop)
            {
                try
                {
                    Log.LogEvent("Work loop", $"Worker#{workerID}");

                    sw.Restart();
                    bufferHead = 0;
                    jobPath = null;
                    jobRename = null;

                    jobPath = WorkCoordinator.GetNextPrimeJob(this);

                    if (string.IsNullOrEmpty(jobPath))
                        break;

                    Log.LogEvent($"Job {Path.GetFileNameWithoutExtension(jobPath)} assigned.", $"Worker#{workerID}");

                    jobRename = Path.ChangeExtension(jobPath, ".Rprimejob"); //do this first to (hopefully) prevent having it readded to the queue
                    File.Move(jobPath, jobRename);
                    job = PrimeJob.Deserialize(jobRename);
                    

                    Log.LogEvent($"Job fetched. {job.FileVersion}", $"Worker#{workerID}");

                    current = job.Start + job.Progress;
                    max = job.Start + job.Count;

                    current = current % 2 == 0 ? current + 1 : current;

                    while (current < max)
                    {
                        if (ResourceHolder.knownPrimes.Length == 0)
                            isPrime = PrimesMath.IsPrime(current);
                        else
                            isPrime = PrimesMath.IsPrime(current, ResourceHolder.knownPrimes);

                        current += 2;

                        if (!isPrime) continue;

                        primeBuffer[bufferHead++] = current;

                        if (bufferHead >= primeBuffer.Length)
                        {
                            job.Primes.AddRange(primeBuffer);
                            bufferHead = 0;

                            if (shouldStop)
                            {
                                break;
                            }
                        }
                    }

                    if (bufferHead < primeBuffer.Length)
                        job.Primes.AddRange(primeBuffer.GetFirst(bufferHead));

                    if (!shouldStop)
                        SaveJob(jobPath, jobRename);
                    else
                        SaveJobPartial(jobPath, jobRename);

                    sw.Stop();
                }
                catch (Exception e)
                {
                    Log.LogException("Exception while doing work.", $"Worker#{workerID}", e);
                }

                Thread.Sleep(0);
            }

            Log.LogEvent("Exiting work loop.", $"Worker#{workerID}");
        }
        private void SaveJob(string sourcePath, string jobRename)
        {
            try
            {
                string path = Path.Combine(Globals.completeDir, job.Batch.ToString());
                Directory.CreateDirectory(path);
                path = Path.Combine(path, $"{job.Start}.primejob");

                PrimeJob.Serialize(job, path);
            }
            catch (Exception e)
            {
                Log.LogException($"Failed to save job '{job.Start}'.", $"Worker#{workerID}", e);
                File.Move(jobRename, sourcePath);
            }

            File.Delete(jobRename);
        }
        private void SaveJobPartial(string sourcePath, string jobRename)
        {
            try
            {
                PrimeJob.Serialize(job, sourcePath);
            }
            catch (Exception e)
            {
                Log.LogException($"Failed to save job '{job.Start}'.", $"Worker#{workerID}", e);
                File.Move(jobRename, sourcePath);
            }

            File.Delete(jobRename);
        }
    }
}
