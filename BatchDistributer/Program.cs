using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;

namespace Primes.BatchDistributer
{
    class Program
    {
        private static string homePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), pendingPath, completedPath;

        static void Main(string[] args)
        {
            if (!Init())
            {
                Log.LogEvent("Failed to init!", "MainThread");
            }

            ParseArguments(args);
        }



        private static bool Init()
        {
            InitDirectories();

            InitLog();

            return true;
        }
        private static bool InitDirectories()
        {
            homePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
            Directory.CreateDirectory(homePath);

            pendingPath = Path.Combine(homePath, "pending");
            Directory.CreateDirectory(pendingPath);

            completedPath = Path.Combine(homePath, "completed");
            Directory.CreateDirectory(completedPath);

            return true;
        }
        private static bool InitLog()
        {
            Log.InitLog(Path.Combine(homePath, "log.txt"));

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
