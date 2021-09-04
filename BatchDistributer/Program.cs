using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Primes.Common;
using Primes.Common.Files;
using Primes.Common.Net;
using Primes.BatchDistributer.Files;
using Primes.BatchDistributer.Net;

namespace Primes.BatchDistributer
{
    static class Program
    {
        public static ClientReceiver clientReceiver;
        public static ClientWaitQueue clientWaitQueue;
        public static Server server;

        public static WorkerTable workerTable;
        public static BatchTable batchTable;

        private static volatile bool exiting = false;

        private static System.Timers.Timer checkExpiredTimer;
        private static System.Timers.Timer saveDBTimer;



        private static void Main(string[] args)
        {
            if (!Init(30000))
            {
                Log.LogEvent(Log.EventType.Error, "Failed to init!", "MainThread");

                Exit();
            }

            ParseArguments(args);

            if (!StartServing())
                Exit();

            WaitForExitOrCrash();

            Exit();
        }



        private static bool Init(int port)
        {
            InitDirectories();

            InitLog();

            if (!InitDB())
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to init database.", "MainThread");
                return false;
            }

            InitTimers();

            if (!InitNet(port))
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to init networking.", "MainThread");
                return false;
            }

            return true;
        }
        private static bool InitDirectories()
        {
            /* 
             * primes
             * |-pending
             * | \-<pending batches>
             * |-archived
             * | \-<archived batches>
             * |-cache
             * | \-<cached batches>
             * |-sent
             * | \-<sent batches>
             * |-db
             * | |-BatchTable.tbl
             * | \-WorkerTable.tbl
             * \-log.txt
             */

            Paths.homePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
            Directory.CreateDirectory(Paths.homePath);

            Paths.pendingPath = Path.Combine(Paths.homePath, "pending");
            Directory.CreateDirectory(Paths.pendingPath);

            Paths.archivedPath = Path.Combine(Paths.homePath, "archived");
            Directory.CreateDirectory(Paths.archivedPath);

            Paths.cachePath = Path.Combine(Paths.homePath, "cache");
            Directory.CreateDirectory(Paths.cachePath);

            Paths.sentPath = Path.Combine(Paths.homePath, "sent");
            Directory.CreateDirectory(Paths.sentPath);

            Paths.dbPath = Path.Combine(Paths.homePath, "db");
            Directory.CreateDirectory(Paths.dbPath);

            return true;
        }
        private static bool InitLog()
        {
            Log.InitConsole();
            Log.InitLog(Path.Combine(Paths.homePath, "log.txt"));

            return true;
        }
        private static bool InitDB()
        {
            try
            {
                string workerTablePath = Path.Combine(Paths.dbPath, "WorkerTable.tbl");

                if (File.Exists(workerTablePath))
                    workerTable = WorkerTable.Deserialize(File.ReadAllBytes(workerTablePath));
                else
                    workerTable = new WorkerTable();

                string batchTablePath = Path.Combine(Paths.dbPath, "BatchTable.tbl");

                if (File.Exists(batchTablePath))
                    batchTable = BatchTable.Deserialize(File.ReadAllBytes(batchTablePath));
                else
                    batchTable = new BatchTable();
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Error, $"Failed to init DB: {e.Message}.", "MainThread");
                return false;
            }

            return true;
        }
        private static bool InitTimers()
        {
            checkExpiredTimer = new System.Timers.Timer();
            checkExpiredTimer.Elapsed += CheckExpired;
            checkExpiredTimer.AutoReset = true;
            checkExpiredTimer.Interval = Settings.Default.timeBetweenExpiredChecks.TotalMilliseconds;

            checkExpiredTimer.Start();

            saveDBTimer = new System.Timers.Timer();
            saveDBTimer.Elapsed += SaveDB;
            saveDBTimer.AutoReset = true;
            saveDBTimer.Interval = 300000;//every 5 mins

            saveDBTimer.Start();

            return true;
        }
        private static bool InitNet(int port)
        {
            try
            {
                clientReceiver = new ClientReceiver(port);
                clientWaitQueue = new ClientWaitQueue();
                server = new Server();
            }
            catch
            {
                return false;
            }

            return true;
        }
        private static bool ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLowerInvariant();

                switch (arg)
                {
                    default:
                        Log.LogEvent($"Unknown argument: {arg}", "MainThread");
                        break;
                }
            }

            return true;
        }



        private static bool StartServing()
        {
            Log.LogEvent("Starting service...", "MainThread");

            try
            {
                clientReceiver.StartListener();
                server.Start();

                Log.LogEvent("Service stated.", "MainThread");

                return true;
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Fatal, $"Failed to start serving: {e.Message}.", "MainThread");

                return false;
            }
        }
        private static bool WaitForExitOrCrash()
        {
            Log.Print("Press enter to exit...");

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey().Key;

                    if (key == ConsoleKey.Enter)
                        break;
                }

                if (!server.Running)
                    break;

                Thread.Sleep(100);
            }

            return true;
        }



        private static void Exit()
        {
            if (exiting)
            {
                Thread.Sleep(1000);
                return;
            }

            exiting = true;

            Log.LogEvent("Preparing to exit...", "MainThread");

            checkExpiredTimer.Stop();
            saveDBTimer.Stop();

            try
            {
                clientReceiver.StopListener();
                server.Stop();
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Error, $"Failed to properly stop client receiver and server: {e.Message}.", "MainThread");
            }

            SaveDB();

            Log.LogEvent("Exiting...", "MainThread");

            Environment.Exit(0);
        }



        private static void SaveDB()
        {
            byte[] workerTableBytes;
            byte[] batchTableBytes;

            lock (workerTable)
            {
                workerTableBytes = workerTable.Serialize();
            }
            
            lock (batchTable)
            {
                batchTableBytes = batchTable.Serialize();
            }

            try//try saving to expected location
            {
                File.WriteAllBytes(Path.Combine(Paths.dbPath, "WorkerTable.tbl"), workerTableBytes);
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Error, $"Failed to store worker table to normal location: {e.Message}.", "SaveDB");

                try//try dump to home directory
                {
                    File.WriteAllBytes(Path.Combine(Paths.homePath, "WorkerTable.tbl.dmp"), workerTableBytes);
                }
                catch (Exception e1)
                {
                    Log.LogEvent(Log.EventType.Error, $"Failed to dump worker table to home directory: {e1.Message}.", "SaveDB");

                    try//try dump to desktop
                    {
                        File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "WorkerTable.tbl.dmp"), workerTableBytes);
                    }
                    catch (Exception e2)//rest in peace
                    {
                        Log.LogEvent(Log.EventType.Error, $"Failed to dump worker table to desktop: {e2.Message}. Data lost.", "SaveDB");
                    }
                }
            }

            try//try saving expected location
            {
                File.WriteAllBytes(Path.Combine(Paths.dbPath, "BatchTable.tbl"), batchTableBytes);
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Error, $"Failed to store batch table to normal location: {e.Message}.", "SaveDB");

                try//try dump to home directory
                {
                    File.WriteAllBytes(Path.Combine(Paths.homePath, "BatchTable.tbl.dmp"), batchTableBytes);
                }
                catch (Exception e1)
                {
                    Log.LogEvent(Log.EventType.Error, $"Failed to dump batch table to home directory: {e1.Message}.", "SaveDB");

                    try//try dump to desktop
                    {
                        File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BatchTable.tbl.dmp"), batchTableBytes);
                    }
                    catch (Exception e2)//rest in peace
                    {
                        Log.LogEvent(Log.EventType.Error, $"Failed to dump batch table to desktop: {e2.Message}. Data lost.", "SaveDB");
                    }
                }
            }
        }
        private static void SaveDB(object sender, EventArgs e) => SaveDB();
        private static void CheckExpired(object sender, EventArgs e)
        {
            Log.LogEvent("Searching for expired batches.", "MainThread");

            DateTime expire = DateTime.Now - Settings.Default.expireTime;
            int[] indexes = batchTable.FindExpiredBatches(expire, out uint[] batchNumbers, out string[] workerIds);

            for (int i = 0; i < indexes.Length; i++)
            {
                Log.LogEvent(Log.EventType.Warning, $"Expiring batch {batchNumbers[i]} from worker {workerIds[i]}.", "MainThread");

                batchTable.AssignBatch("    ", BatchEntry.BatchStatus.Stored_Ready, indexes[i], BatchTable.TimeSetting.ResetBoth);
            }

            SaveDB();
        }
    }
}
