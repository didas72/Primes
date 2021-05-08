using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes.Service
{
    class PrimesWork
    {
        public static string home, jobsPath, completePath;

        public static JobDistributer distributer;



        private static void ApplyComputeMode()
        {
            log.WriteEntry($"Applying computeMode {computeMode}");

            switch (computeMode)
            {
                case ServiceComputeMode.None:

                    if (distributer.Working())
                        distributer.StopWork();

                    break;

                case ServiceComputeMode.Waiting_Idle:

                    if (distributer.Working())
                        distributer.StopWork();

                    break;

                case ServiceComputeMode.Compute_Partial:

                    int threadCount = (int)Math.Max(1, Math.Floor(Environment.ProcessorCount * partialThreadsMult));

                    log.WriteEntry($"Calculated threadCount {threadCount}");

                    if (distributer.Workers.Length != threadCount)
                        RescaleWorkers(threadCount);

                    if (!distributer.Working())
                        distributer.StartWork();

                    break;

                case ServiceComputeMode.Compute_Full:

                    threadCount = (int)Math.Max(1, Math.Floor(Environment.ProcessorCount * fullThreadsMult));

                    log.WriteEntry($"Calculated threadCount {threadCount}");

                    if (distributer.Workers.Length != threadCount)
                        RescaleWorkers(threadCount);

                    if (!distributer.Working())
                        distributer.StartWork();

                    break;

                case ServiceComputeMode.Waiting_Switch_Full:

                    if (distributer.Working())
                        distributer.StopWork();

                    break;

                case ServiceComputeMode.Waiting_Switch_Partial:

                    if (distributer.Working())
                        distributer.StopWork();

                    break;
            }
        }

        private static void RescaleWorkers(int workerCount)
        {
            distributer.RescaleWorkers(workerCount);

            distributer.StartWork();
        }
    }
}
