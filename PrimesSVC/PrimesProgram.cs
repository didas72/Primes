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

        private static Timer updatesTimer;
        private static Timer computeModeSwitchTimer;

        private static PerformanceCounter totalCPU;
        private static PerformanceCounter localCPU;

        private volatile static ServiceComputeMode computeMode = ServiceComputeMode.None;
        private volatile static ServiceMasterMode masterMode = ServiceMasterMode.None;



        public static void Start()
        {
            masterMode = ServiceMasterMode.Starting;

            Init();

            updatesTimer.Start();

            computeMode = ServiceComputeMode.Waiting_Idle;

            log.WriteEntry("Started");
        }
        public static void Stop()
        {
            log.WriteEntry("Stopping");

            masterMode = ServiceMasterMode.Stopping;

            updatesTimer.Stop();
            computeModeSwitchTimer.Stop();

            System.Threading.Thread.Sleep(10);

            Cleanup();
        }



        public static void UpdatesTimerElapsed(object sender, ElapsedEventArgs e)
        {
            log.WriteEntry("Updating service computeMode");
            UpdateServiceComputeMode();
        }
        public static void ModeSwitchElapsed(object sender, ElapsedEventArgs e)
        {
            log.WriteEntry("Switching computeMode");

            if (computeMode == ServiceComputeMode.Waiting_Switch_Full)
                computeMode = ServiceComputeMode.Compute_Full;
            else if (computeMode == ServiceComputeMode.Waiting_Switch_Partial)
                computeMode = ServiceComputeMode.Compute_Partial;
            else return;

            ApplyMode();
        }



        public static void AllJobsDistributed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }



        private static void UpdateServiceComputeMode()
        {
            GetCPUUsage(out float our, out float total);

            float others = total - our;

            switch (computeMode)
            {
                case ServiceComputeMode.Waiting_Idle:

                    if (others < partialIdleThreshold) //if enough free CPU time, start switch to partial compute
                    {
                        computeMode = ServiceComputeMode.Waiting_Switch_Partial;
                        computeModeSwitchTimer.Start();
                    }

                    break;

                case ServiceComputeMode.Waiting_Switch_Partial:

                    if (others >= partialIdleThreshold) //if not enough free CPU time, abort switch
                    {
                        computeMode = ServiceComputeMode.Waiting_Idle;
                        computeModeSwitchTimer.Stop();
                    }

                    break;

                case ServiceComputeMode.Compute_Partial:

                    if (others >= partialIdleThreshold) //if not enough CPU time, return to idle
                    {
                        computeMode = ServiceComputeMode.Waiting_Idle;
                    }
                    else if (others < fullIdleThreshold) //if enought free CPU time, start switch to full compute
                    {
                        computeMode = ServiceComputeMode.Waiting_Switch_Full;
                        computeModeSwitchTimer.Start();
                    }

                    break;

                case ServiceComputeMode.Waiting_Switch_Full:

                    if (others >= partialIdleThreshold) //if not enough CPU time, return to idle
                    {
                        computeMode = ServiceComputeMode.Waiting_Idle;
                        computeModeSwitchTimer.Stop();
                    }
                    else if (others >= fullIdleThreshold) //if not enough CPU time, return to partial compute
                    {
                        computeMode = ServiceComputeMode.Waiting_Switch_Partial;
                        computeModeSwitchTimer.Stop();
                        computeModeSwitchTimer.Start(); //reset to wait for further usage
                    }

                    break;

                case ServiceComputeMode.Compute_Full:

                    if (others >= partialIdleThreshold) //if not enough CPU time, return to idle
                    {
                        computeMode = ServiceComputeMode.Waiting_Idle;
                    }
                    else if (others >= fullIdleThreshold) //if not enough CPU time, return to partial compute
                    {
                        computeMode = ServiceComputeMode.Waiting_Switch_Partial;
                        computeModeSwitchTimer.Start(); //reset to wait for further usage
                    }

                    break;
            }

            ApplyMode();
        }



        public static void Init()
        {
            InitDirectories();

            InitDistributer();

            InitTimers();

            InitPerformanceCounters();

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

                distributer.AllJobsDistributed += AllJobsDistributed;

                log.WriteEntry("Distributer initialized.");
            }
            catch (Exception e)
            {
                log.WriteEntry($"InitDistributer() error: {e.Message}", EventLogEntryType.Error);
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

                computeModeSwitchTimer = new Timer
                {
                    Interval = 60000, //60 seconds of idle to allow to start work
                    AutoReset = false,
                    Enabled = false,
                };

                computeModeSwitchTimer.Elapsed += ModeSwitchElapsed;

                log.WriteEntry("Timers initialized.");
            }
            catch (Exception e)
            {
                log.WriteEntry($"InitTimers() error: {e.Message}", EventLogEntryType.Error);
            }
        }
        public static void InitPerformanceCounters()
        {
            localCPU = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
            totalCPU = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            localCPU.NextValue(); //do first retrieve, always = 0
            totalCPU.NextValue();

            log.WriteEntry("Performance counters initialized.");
        }
        public static void Cleanup()
        {
            try
            {
                updatesTimer.Dispose();
                computeModeSwitchTimer.Dispose();
            }
            catch (Exception e)
            {
                log.WriteEntry($"Cleanup() error: {e.Message}", EventLogEntryType.Error);
            }

            log.WriteEntry("Closing log.");

            log.Dispose();
        }



        public static void GetCPUUsage(out float local, out float total)
        {
            local = 0f;
            total = 0f;

            try
            {
                local = localCPU.NextValue();
                total = totalCPU.NextValue();
            }
            catch (Exception e)
            {
                log.WriteEntry($"GetCPUUsage() error: {e.Message}", EventLogEntryType.Error);
            }
        }
    }

    public enum ServiceMasterMode
    {
        None,
        Starting,
        Stopping,
        Compute_CPU_Usage,
        Compute_Schedule,
        Idle_No_Jobs,
        Idle_User_Request,
        Idle,
    }
    public enum ServiceComputeMode
    {
        None,
        Waiting_Idle,
        Waiting_Switch_Partial,
        Waiting_Switch_Full,
        Compute_Partial,
        Compute_Full,
    }
}
