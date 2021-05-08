using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceProcess;

namespace Primes.Service
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }



        protected override void OnStart(string[] args)
        {
            CheckSource();

            PrimesProgram.log = new EventLog() { Source = "PrimesSVC", Log = "PrimesSVCLog" };

            PrimesProgram.log.WriteEntry("Log started.");

            PrimesProgram.Start();
        }
        protected override void OnStop()
        {
            PrimesProgram.Stop();
        }
        protected override void OnContinue()
        {
            //continue
        }
        protected override void OnPause()
        {
            //pause
        }
        protected override void OnShutdown()
        {
            OnStop();
        }

        private static void CheckSource()
        {
            if (!EventLog.SourceExists("PrimesSVC"))
                EventLog.CreateEventSource("PrimesSVC", "PrimesSVCLog");
        }
    }
}
