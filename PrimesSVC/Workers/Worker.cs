using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Extensions;

using Primes.Common;
using Primes.Common.Files;

namespace Primes.SVC
{
    internal class Worker
    {
        private ulong[] primeBuffer;
        private int bufferHead;
        private PrimeJob job;
        private readonly Stopwatch sw;
        private readonly Semaphore stopControl;
        private Thread selfThread;



        public Worker(int primeBufferSize)
        {
            stopControl = new Semaphore(0, 1);
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



        private void WorkLoop()
        {
            while (true)
            {
                if (stopControl.WaitOne(0)) break;

                sw.Restart();
                bufferHead = 0;
                job = WorkCoordinator.GetNextPrimeJob();
                
                if (job == null) break;

                throw new NotImplementedException();

                sw.Stop();
            }

            sw.Stop();
        }
    }
}
