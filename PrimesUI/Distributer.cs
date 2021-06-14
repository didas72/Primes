using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Primes.Common;
using Primes.Common.Files;

namespace PrimesUI
{
    class Distributer
    {
        private Queue<string> pendingJobs = new Queue<string>();

        private readonly string jobsPath;

        public Distributer(string jobsPath)
        {
            this.jobsPath = jobsPath;
        }

        public bool GetPendingPrimeJob(out PrimeJob job)
        {
            job = PrimeJob.Empty;

            lock (pendingJobs)
            {
                if (pendingJobs.Count == 0)
                    Utils.GetDoableJobs(jobsPath);

                if (pendingJobs.Count == 0)
                    return false;

                job = PrimeJob.Deserialize(pendingJobs.Dequeue());
            }

            return true;
        }
    }
}
