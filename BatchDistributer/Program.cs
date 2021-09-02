using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Primes.BatchDistributer.Files;
using Primes.BatchDistributer.Net;

namespace Primes.BatchDistributer
{
    static class Program
    {
        public static ClientReceiver clientReceiver;
        public static ClientWaitQueue clientWaitQueue;

        public static WorkerTable workerTable;
        public static BatchTable batchTable;



        private static void Main(string[] args)
        {
            if (!Init(30000))
            {
                Log.LogEvent("Failed to init!", "MainThread");
            }

            ParseArguments(args);
        }



        private static bool Init(int port)
        {
            InitDirectories();

            InitLog();

            InitDB();

            InitNet(port);

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

            Paths.dbPath = Path.Combine(Paths.homePath, "db");
            Directory.CreateDirectory(Paths.dbPath);

            return true;
        }
        private static bool InitLog()
        {
            Log.InitLog(Path.Combine(Paths.homePath, "log.txt"));

            return true;
        }
        private static void InitDB()
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
        private static bool InitNet(int port)
        {
            clientReceiver = new ClientReceiver(port);

            clientWaitQueue = new ClientWaitQueue();

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
    }
}
