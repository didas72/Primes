using System;
using System.IO;
using System.Threading;

using Primes.Common;

namespace Primes.Updater
{
    class Program
    {
        private static string tmpDir, homeDir;

        private static bool updateSelf;
        private static bool updatePrimes;

        private static volatile bool exiting = false;



        private static void Main(string[] args)
        {
            if (!Init())
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to init.", "MainThread");
                Exit();
            }

            if (!ParseArguments(args))
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to parse arguments.", "MainThread");
                Exit();
            }

            if (updateSelf)
            {
                Updater.UpdateResult updateSelfRet = Updater.UpdateSelf(tmpDir);
                Console.WriteLine($"Update self return: {updateSelfRet}.");
            }

            if (updatePrimes)
            {
                Updater.UpdateResult updatePrimesRet = Updater.UpdatePrimes(tmpDir);
                Console.WriteLine($"Update primes return: {updatePrimesRet}.");
            }

            CleanUp();

            Log.LogEvent("Updates completed successfully. Press any key to close the program.", "MainThread");
            Utils.WaitForKey();
            Environment.Exit(0);
        }



        private static bool Init()
        {
            Log.InitConsole();

            if (!InitDirs())
                return false;

            if (!InitLog())
                return false;

            return true;
        }
        private static bool InitDirs()
        {
            Console.WriteLine("Setting up directories...");

            homeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
            Directory.CreateDirectory(homeDir);

            tmpDir = Path.Combine(homeDir, "tmp");
            Utils.DeleteDirectory(tmpDir);
            Directory.CreateDirectory(tmpDir);

            Console.WriteLine("Directories set up.");

            return true;
        }
        private static bool InitLog()
        {
            Log.InitLog(homeDir, "updateLog.txt");
            Log.InitConsole();

            return true;
        }
        private static bool ParseArguments(string[] args)
        {
            updateSelf = true;
            updatePrimes = true;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "/?":
                        Log.Print("Arguments:");
                        Log.Print("'-ns' - Disables self update.");
                        Log.Print("'-np' - Disables primes update.");
                        Environment.Exit(0);
                        break;

                    case "-ns":
                        updateSelf = false;
                        break;

                    case "-np":
                        updatePrimes = false;
                        break;

                    default:
                        Console.WriteLine("Invalid argument: " + args[i]);
                        return false;
                }
            }

            return true;
        }



        private static void CleanUp()
        {
            Log.LogEvent("Cleaning up...", "MainThread");

            Utils.DeleteDirectory(tmpDir);
        }
        private static void Exit()
        {
            if (exiting)
            {
                Thread.Sleep(4000);
                return;
            }

            exiting = true;

            Log.Print("Something went wrong!");

            Thread.Sleep(3000);

            Environment.Exit(1);
        }
    }
}
