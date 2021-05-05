using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO;
using System.Timers;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes.Service
{
    public static class PrimesProgram
    {
        private const float fullIdleThreshold = 10f;
        private const float partialIdleThreshold = 55f;
        private const float partialThreadsMult = .4f; //never use more than the available CPU time (so we can monitor other apps for cpu time)
        private const float fullThreadsMult = .8f; //never use all threads (so we can monitor other apps for cpu time)



        public static ulong[] knowPrimes = null;
        public static EventLog log;
        public static string home, jobsPath, completePath;

        public static JobDistributer distributer;

        public static NamedPipeServerStream pipe;

        private static Timer updatesTimer;
        private static Timer modeSwitchTimer;

        private volatile static ServiceMode mode = ServiceMode.None;



        public static void Start()
        {
            mode = ServiceMode.Starting;

            Init();

            updatesTimer.Start();

            log.WriteEntry("Started");
        }
        public static void Stop()
        {
            log.WriteEntry("Stopping");

            mode = ServiceMode.Stopping;

            updatesTimer.Stop();
            modeSwitchTimer.Stop();

            Cleanup();
        }



        public static void UpdatesTimerElapsed(object sender, ElapsedEventArgs e)
        {
            log.WriteEntry("Updating service mode");
            UpdateServiceMode();

            log.WriteEntry("Checking pipe");
            CheckPipe();
        }
        public static void ModeSwitchElapsed(object sender, ElapsedEventArgs e)
        {
            log.WriteEntry("Switching mode");

            if (mode == ServiceMode.Waiting_Switch_Full)
                mode = ServiceMode.Compute_Full;
            else if (mode == ServiceMode.Waiting_Switch_Partial)
                mode = ServiceMode.Compute_Partial;
            else return;

            ApplyMode();
        }



        private static void UpdateServiceMode()
        {
            GetCPUUsage(out float our, out float total);

            float others = total - our;

            switch (mode)
            {
                case ServiceMode.Waiting_Idle:

                    if (others < partialIdleThreshold) //if enough free CPU time, start switch to partial compute
                    {
                        mode = ServiceMode.Waiting_Switch_Partial;
                        modeSwitchTimer.Start();
                    }

                    break;

                case ServiceMode.Waiting_Switch_Partial:

                    if (others >= partialIdleThreshold) //if not enough free CPU time, abort switch
                    {
                        mode = ServiceMode.Waiting_Idle;
                        modeSwitchTimer.Stop();
                    }

                    break;

                case ServiceMode.Compute_Partial:

                    if (others >= partialIdleThreshold) //if not enough CPU time, return to idle
                    {
                        mode = ServiceMode.Waiting_Idle;
                    }
                    else if (others < fullIdleThreshold) //if enought free CPU time, start switch to full compute
                    {
                        mode = ServiceMode.Waiting_Switch_Full;
                        modeSwitchTimer.Start();
                    }

                    break;

                case ServiceMode.Waiting_Switch_Full:

                    if (others >= partialIdleThreshold) //if not enough CPU time, return to idle
                    {
                        mode = ServiceMode.Waiting_Idle;
                        modeSwitchTimer.Stop();
                    }
                    else if (others >= fullIdleThreshold) //if not enough CPU time, return to partial compute
                    {
                        mode = ServiceMode.Waiting_Switch_Partial;
                        modeSwitchTimer.Stop();
                        modeSwitchTimer.Start(); //reset to wait for further usage
                    }

                    break;

                case ServiceMode.Compute_Full:

                    if (others >= partialIdleThreshold) //if not enough CPU time, return to idle
                    {
                        mode = ServiceMode.Waiting_Idle;
                    }
                    else if (others >= fullIdleThreshold) //if not enough CPU time, return to partial compute
                    {
                        mode = ServiceMode.Waiting_Switch_Partial;
                        modeSwitchTimer.Start(); //reset to wait for further usage
                    }

                    break;
            }

            ApplyMode();
        }
        private static void ApplyMode()
        {
            log.WriteEntry($"Applying mode {mode}");

            switch(mode)
            {
                case ServiceMode.Waiting_Idle:

                    if (distributer.Working())
                        distributer.StopWork();

                    break;

                case ServiceMode.Compute_Partial:

                    int threadCount = (int)Math.Min(1, Math.Floor(Environment.ProcessorCount * partialThreadsMult));

                    if (distributer.Workers.Length != threadCount)
                    {
                        distributer.RescaleWorkers(threadCount);

                        distributer.StartWork();
                    }

                    if (!distributer.Working())
                        distributer.StartWork();

                    break;

                case ServiceMode.Compute_Full:

                    threadCount = (int)Math.Min(1, Math.Floor(Environment.ProcessorCount * fullThreadsMult));

                    if (distributer.Workers.Length != threadCount)
                    {
                        distributer.RescaleWorkers(threadCount);

                        distributer.StartWork();
                    }

                    if (!distributer.Working())
                        distributer.StartWork();

                    break;

                case ServiceMode.Waiting_Switch_Full:

                    if (distributer.Working())
                        distributer.StopWork();

                    break;

                case ServiceMode.Waiting_Switch_Partial:

                    if (distributer.Working())
                        distributer.StopWork();

                    break;

                case ServiceMode.Stopping:

                    if (distributer.Working())
                        distributer.StopWork();

                    break;
            }
        }



        private static void CheckPipe()
        {
            /*try
            {
                if (pipe.Position + 1 != pipe.Length) //pending reads
                {
                    throw new NotImplementedException("Pipe handling has not been implemented yet.");
                }
            }
            catch (Exception e)
            {
                log.WriteEntry(e.Message);
            }*/
        }



        public static void Init()
        {
            InitDirectories();

            InitDistributer();

            InitPipe();

            InitTimers();

            log.WriteEntry("Service started.");
        }
        public static void InitDirectories()
        {
            try
            {
                home = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
                Directory.CreateDirectory(home);

                jobsPath = Path.Combine(home, "jobs");
                Directory.CreateDirectory(jobsPath);

                completePath = Path.Combine(home, "complete");
                Directory.CreateDirectory(completePath);

                log.WriteEntry("Directories initialized.");
            }
            catch (Exception e)
            {
                log.WriteEntry($"InitDirectories() error: {e.Message}", EventLogEntryType.Error);
            }
        }
        public static void InitDistributer()
        {
            try
            {
                distributer = new JobDistributer(jobsPath, completePath, Environment.ProcessorCount);

                log.WriteEntry("Distributer initialized.");
            }
            catch (Exception e)
            {
                log.WriteEntry($"InitDistributer() error: {e.Message}", EventLogEntryType.Error);
            }
        }
        public static void InitPipe()
        {
            try
            {
                pipe = new NamedPipeServerStream("Didas72PrimesService", PipeDirection.InOut);

                log.WriteEntry("Pipe initialized.");
            }
            catch (Exception e)
            {
                log.WriteEntry($"InitPipe() error: {e.Message}", EventLogEntryType.Error);
            }
        }
        public static void InitTimers()
        {
            try
            {
                updatesTimer = new Timer
                {
                    Interval = 10000, //check every 10 seconds
                    AutoReset = true,
                };

                updatesTimer.Elapsed += UpdatesTimerElapsed;

                modeSwitchTimer = new Timer
                {
                    Interval = 60000, //60 seconds of idle to allow to start work
                    AutoReset = false,
                };

                updatesTimer.Elapsed += ModeSwitchElapsed;

                log.WriteEntry("Timers initialized.");
            }
            catch (Exception e)
            {
                log.WriteEntry($"InitTimers() error: {e.Message}", EventLogEntryType.Error);
            }
        }
        public static void Cleanup()
        {
            try
            {
                updatesTimer.Dispose();
                modeSwitchTimer.Dispose();

                log.WriteEntry("Closing log.");

                log.Dispose();
            }
            catch (Exception e)
            {
                log.WriteEntry($"Cleanup() error: {e.Message}", EventLogEntryType.Error);
            }
        }



        private static void GetCPUUsage(out float our, out float total)
        {
            try
            {
                our = new PerformanceCounter("Processor", "% Processor Time", "PrimesSVC.exe").NextValue();
                total = new PerformanceCounter("Processor", "% Processor Time", "_Total").NextValue();
            }
            catch (Exception e)
            {
                log.WriteEntry($"GetCPUUsage() error: {e.Message}", EventLogEntryType.Error);

                our = 0f;
                total = 0f;
            }
        }



        public enum ServiceMode
        {
            None,
            Starting,
            Stopping,
            Waiting_Idle,
            Waiting_Switch_Partial,
            Waiting_Switch_Full,
            Compute_Partial,
            Compute_Full,
        }
    }
}
