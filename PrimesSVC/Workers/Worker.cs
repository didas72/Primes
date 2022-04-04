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
        private readonly ulong[] primeBuffer;
        private int bufferHead;
        private PrimeJob job;
        private bool isPrime;
        private ulong current, max;
        private readonly Stopwatch sw;
        private Semaphore stopControl;
        private Thread selfThread;



        public Worker(int primeBufferSize)
        {
            primeBuffer = new ulong[primeBufferSize];
            sw = new();
            job = null;
            selfThread = null;
        }



        public void Start()
        {
            if (selfThread == null)
                selfThread = new Thread(WorkLoop);
            if (!selfThread.IsAlive)
                selfThread.Start();
        }
        public void Stop()
        {
            stopControl.WaitOne();
        }

        public bool IsRunning()
        {
            if (selfThread == null) return false;
            return selfThread.IsAlive;
        }



        private void WorkLoop()
        {
            stopControl = new Semaphore(0,1);

            while (true)
            {
                sw.Restart();
                bufferHead = 0;
                job = WorkCoordinator.GetNextPrimeJob();
                
                if (job == null) break;

                current = job.Start + job.Progress;
                max = job.Start + job.Count;

                current = current % 2 == 0 ? current + 1 : current;
                
                while (current < max)
                {
                    if (ResourceHolder.knownPrimes.Length == 0)
                        isPrime = PrimesMath.IsPrime(current);
                    else
                        isPrime = PrimesMath.IsPrime(current, ResourceHolder.knownPrimes);

                    if (!isPrime) continue;

                    primeBuffer[bufferHead++] = current;

                    if (bufferHead >= primeBuffer.Length)
                    {
                        job.Primes.AddRange(primeBuffer);
                        bufferHead = 0;

                        if (stopControl.WaitOne(0)) break;
                    }

                    current += 2;
                }

                if (bufferHead < primeBuffer.Length)
                    job.Primes.AddRange(primeBuffer.GetFirst(bufferHead));

                SaveJob();

                sw.Stop();
                if (stopControl.WaitOne(0)) break;
            }
        }
        private void SaveJob()
        {
            try
            {
                string path = Path.Combine(Globals.completeDir, $"{job.Start}.primejob");

                PrimeJob.Serialize(job, path);
            }
            catch (Exception e)
            {
                Log.LogException($"Failed to save job '{job.Start}'.", "Worker", e);
            }
        }
    }
}
