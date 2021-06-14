using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;

using Primes.Common;
using Primes.Common.Files;

namespace PrimesUI
{
    public static class PrimesProgram
    {
        public static bool resourcesLoaded = false;
        public static ulong[] knownPrimes;



        private static Worker[] workers;

        private static Distributer distributer;

        private static string homeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");







        #region Init
        public static void Init()
        {
            InitDirs();

            InitComponents();
        }

        private static void InitDirs()
        {
            Directory.CreateDirectory(homeDir);
            Directory.CreateDirectory(Path.Combine(homeDir, "jobs"));
            Directory.CreateDirectory(Path.Combine(homeDir, "complete"));
            Directory.CreateDirectory(Path.Combine(homeDir, "resources"));
        }

        private static void InitComponents()
        {
            distributer = new Distributer(Path.Combine(homeDir, "jobs"));

            InitWorkers();
        }

        private static void InitWorkers()
        {
            workers = new Worker[Settings.Default.Threads];

            for (int i = 0; i < workers.Length; i++)
            {
                workers[i] = new Worker(i, (int)Settings.Default.PrimeBufferSize);
                workers[i].OnStop += OnWorkerStop;
            }
        }
        #endregion




        public static void ApplySettings()
        {
            Pause();

            InitWorkers();

            Resume();
        }

        public static void Resume()
        {
            for (int i = 0; i < workers.Length; i++)
            {
                workers[i].StartWork(ref distributer);
            }
        }

        public static void Pause()
        {
            for (int i = 0; i < workers.Length; i++)
            {
                workers[i].StopAndJoin();
            }
        }

        public static void Exit()
        {
            Pause();

            Application.Exit();
        }



        #region FormalEventHandlers
        public static void OnUITimer(object sender, EventArgs e)
        {
            //TODO do UI updates
        }

        public static void OnWorkerStop(object sender, WorkerStoppedEventArgs e)
        {
            //TODO handle event
        }
        #endregion
    }
}
