using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.IO;
using System.IO.Pipes;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes.Service
{
    static class Program
    {
        public static ulong[] knowPrimes = null;
        public static EventLog log;

        public static JobDistributer distributer;

        public static NamedPipeServerStream pipe;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service1()
            };
            ServiceBase.Run(ServicesToRun);

            Init();

            //main code

            Cleanup();
        }



        static void Init()
        {
            if (!EventLog.SourceExists("PrimesSVC"))
                EventLog.CreateEventSource("PrimesSVC", "PrimesSVCLog");

            Thread.Sleep(200);
            log = new EventLog() { Source = "PrimesSVC" };


            log.WriteEntry("Log started.");



            string home = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
            Directory.CreateDirectory(home);

            string jobsPath = Path.Combine(home, "jobs");
            Directory.CreateDirectory(jobsPath);

            string completePath = Path.Combine(home, "complete");
            Directory.CreateDirectory(completePath);

            log.WriteEntry("Directories initialized.");



            distributer = new JobDistributer(jobsPath, completePath, Environment.ProcessorCount);

            log.WriteEntry("Distributer initialized.");



            log.WriteEntry("Service started.");
        }
        static void Cleanup()
        {
            log.Dispose();
        }
    }
}
