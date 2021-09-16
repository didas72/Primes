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
            if (!Init(Settings.Default.port))
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
                Log.LogEvent(Log.EventType.Error, "Failed to init DB.", "MainThread");
                Log.LogException("Failed to init DB.", "MainThread", e);

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
                    case "/?":
                    case "-?":
                        Log.Print("Arguments:");
                        Log.Print("'-p P' - Port to use, P being the desired port. P must be an integer between 1 and 65535");
                        Log.Print("'-ec T' - Time between expire checks, T being the desired value. T must be presented in the following format: 'HH:MM:SS'. T must be between 0:5:0 and 6:0:0");
                        Log.Print("'-et T' - Time allowed before expires, T being the desired value. T must be presented in the following format: 'HH:MM:SS'. T must be between 24:0:0 and 720:0:0");
                        Log.Print("'-b B' - Max batches allowed per user, B being the desired value. B must be and integer between 1 and 65535");
                        Log.Print("'-mw M' - Max time allowed to wait for a message, in milliseconds, M being the desired value. M must be an integer between 100 and 60000");
                        Log.Print("'-mcc C' - Max number of consecutive serving failures, C being the desired value. C must be an integer between 2 and 1000");
                        break;

                    case "-p":
                        if (args.Length >= i)
                        {
                            if (ushort.TryParse(args[i + 1], out ushort port))
                            {
                                if (port != 0)
                                {
                                    Settings.Default.port = port;
                                    Settings.Default.Save();
                                }
                                else
                                {
                                    Log.Print("Argument '-p' must be followed by a integer between 1 and 65535");
                                    return false;
                                }
                            }
                            else
                            {
                                Log.Print("Argument '-p' must be followed by a integer between 1 and 65535");
                                return false;
                            }
                        }
                        else
                        {
                            Log.Print("Argument '-p' must be followed by a integer between 1 and 65535");
                            return false;
                        }
                        break;

                    case "-ec":
                        if (args.Length >= i)
                        {
                            if (TimeSpan.TryParse(args[i + 1], out TimeSpan span))
                            {
                                if (span.TotalMinutes >= 5 && span.TotalMinutes <= 360)
                                {
                                    Settings.Default.timeBetweenExpiredChecks = span;
                                    Settings.Default.Save();
                                }
                                else
                                {
                                    Log.Print("Argument '-ec' must be followed by a value presented in the following format: 'HH:MM:SS' and between 0:5:0 and 6:0:0");
                                    return false;
                                }
                            }
                            else
                            {
                                Log.Print("Argument '-ec' must be followed by a value presented in the following format: 'HH:MM:SS' and between 0:5:0 and 6:0:0");
                                return false;
                            }
                        }
                        else
                        {
                            Log.Print("Argument '-ec' must be followed by a value presented in the following format: 'HH:MM:SS' and between 0:5:0 and 6:0:0");
                            return false;
                        }
                        break;

                    case "-et":
                        if (args.Length >= i)
                        {
                            if (TimeSpan.TryParse(args[i + 1], out TimeSpan span))
                            {
                                if (span.TotalHours >= 24 && span.TotalHours <= 720)
                                {
                                    Settings.Default.expireTime = span;
                                    Settings.Default.Save();
                                }
                                else
                                {
                                    Log.Print("Argument '-et' must be followed by a value presented in the following format: 'HH:MM:SS' and between 24:0:0 and 720:0:0");
                                    return false;
                                }
                            }
                            else
                            {
                                Log.Print("Argument '-et' must be followed by a value presented in the following format: 'HH:MM:SS' and between 24:0:0 and 720:0:0");
                                return false;
                            }
                        }
                        else
                        {
                            Log.Print("Argument '-et' must be followed by a value presented in the following format: 'HH:MM:SS' and between 24:0:0 and 720:0:0");
                            return false;
                        }
                        break;

                    case "-b":
                        if (args.Length >= i)
                        {
                            if (ushort.TryParse(args[i + 1], out ushort bacthes))
                            {
                                if (bacthes != 0)
                                {
                                    Settings.Default.maxBatchesPerWorker = bacthes;
                                    Settings.Default.Save();
                                }
                                else
                                {
                                    Log.Print("Argument '-b' must be followed by a integer between 1 and 65535");
                                    return false;
                                }
                            }
                            else
                            {
                                Log.Print("Argument '-b' must be followed by a integer between 1 and 65535");
                                return false;
                            }
                        }
                        else
                        {
                            Log.Print("Argument '-b' must be followed by a integer between 1 and 65535");
                            return false;
                        }
                        break;

                    case "-mw":
                        if (args.Length >= i)
                        {
                            if (ushort.TryParse(args[i + 1], out ushort millis))
                            {
                                if (millis >= 100 && millis <= 60000)
                                {
                                    Settings.Default.maxWaitMessage = millis;
                                    Settings.Default.Save();
                                }
                                else
                                {
                                    Log.Print("Argument '-mw' must be followed by a integer between 100 and 60000");
                                    return false;
                                }
                            }
                            else
                            {
                                Log.Print("Argument '-mw' must be followed by a integer between 100 and 60000");
                                return false;
                            }
                        }
                        else
                        {
                            Log.Print("Argument '-mw' must be followed by a integer between 100 and 60000");
                            return false;
                        }
                        break;

                    case "-mcc":
                        if (args.Length >= i)
                        {
                            if (ushort.TryParse(args[i + 1], out ushort crashes))
                            {
                                if (crashes >= 2 && crashes <= 1000)
                                {
                                    Settings.Default.maxConsecutiveCrashes = crashes;
                                    Settings.Default.Save();
                                }
                                else
                                {
                                    Log.Print("Argument '-mcc' must be followed by a integer between 2 and 1000");
                                    return false;
                                }
                            }
                            else
                            {
                                Log.Print("Argument '-mcc' must be followed by a integer between 2 and 1000");
                                return false;
                            }
                        }
                        else
                        {
                            Log.Print("Argument '-mcc' must be followed by a integer between 2 and 1000");
                            return false;
                        }
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
                Log.LogEvent(Log.EventType.Fatal, "Failed to start serving.", "MainThread");
                Log.LogException("Failed to start serving.", "MainThread", e);

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
                Log.LogEvent(Log.EventType.Error, "Failed to properly stop client receiver and server.", "MainThread");
                Log.LogException("Failed to properly stop client receiver and server.", "MainThread", e);
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
                Log.LogEvent(Log.EventType.Error, "Failed to store worker table to normal location.", "SaveDB", true, false);
                Log.LogException("Failed to store worker table to normal location.", "SaveDB", e);

                try//try dump to home directory
                {
                    File.WriteAllBytes(Path.Combine(Paths.homePath, "WorkerTable.tbl.dmp"), workerTableBytes);
                }
                catch (Exception e1)
                {
                    Log.LogEvent(Log.EventType.Error, "Failed to dump worker table to home directory.", "SaveDB", true, false);
                    Log.LogException("Failed to dump worker table to home directory.", "SaveDB", e1);

                    try//try dump to desktop
                    {
                        File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "WorkerTable.tbl.dmp"), workerTableBytes);
                    }
                    catch (Exception e2)//rest in peace
                    {
                        Log.LogEvent(Log.EventType.Error, "Failed to dump worker table to desktop. Data lost.", "SaveDB", true, false);
                        Log.LogException("Failed to dump worker table to desktop. Data lost.", "SaveDB", e2);
                    }
                }
            }

            try//try saving expected location
            {
                File.WriteAllBytes(Path.Combine(Paths.dbPath, "BatchTable.tbl"), batchTableBytes);
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Error, "Failed to store batch table to normal location.", "SaveDB", true, false);
                Log.LogException("Failed to store batch table to normal location.", "SaveDB", e);

                try//try dump to home directory
                {
                    File.WriteAllBytes(Path.Combine(Paths.homePath, "BatchTable.tbl.dmp"), batchTableBytes);
                }
                catch (Exception e1)
                {
                    Log.LogEvent(Log.EventType.Error, "Failed to dump batch table to home directory.", "SaveDB", true, false);
                    Log.LogException("Failed to dump batch table to home directory.", "SaveDB", e1);

                    try//try dump to desktop
                    {
                        File.WriteAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BatchTable.tbl.dmp"), batchTableBytes);
                    }
                    catch (Exception e2)//rest in peace
                    {
                        Log.LogEvent(Log.EventType.Error, "Failed to dump batch table to desktop. Data lost.", "SaveDB", true, false);
                        Log.LogException("Failed to dump batch table to desktop. Data lost.", "SaveDB", e2);
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
