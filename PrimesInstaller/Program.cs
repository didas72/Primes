using System;
using System.IO;
using System.Threading;

using DidasUtils;
using DidasUtils.Logging;

namespace Primes.Installer
{
    class Program
    {
        public static string tmpDir, homeDir, installDir;

        private static bool updateSelf;
        private static bool updatePrimes;

        private static volatile bool exiting;



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
                Log.LogEvent($"Update self return: {updateSelfRet}.", "MainThread");

                if (updateSelfRet == Updater.UpdateResult.Failed_Version_Check || updateSelfRet == Updater.UpdateResult.Failed_Extraction || updateSelfRet == Updater.UpdateResult.Failed_Download)
                    Exit();
            }

            if (updatePrimes)
            {
                Updater.UpdateResult updatePrimesRet = Updater.UpdatePrimes(tmpDir, installDir);
                Log.LogEvent($"Update primes return: {updatePrimesRet}.", "MainThread");

                if (updatePrimesRet == Updater.UpdateResult.Failed_Version_Check || updatePrimesRet == Updater.UpdateResult.Failed_Extraction || updatePrimesRet == Updater.UpdateResult.Failed_Download)
                    Exit();
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
            Directory.Delete(tmpDir, true);
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
                        Log.Print("'-p P' - Specifies install path.");
                        Environment.Exit(0);
                        break;

                    case "-ns":
                        updateSelf = false;
                        break;

                    case "-np":
                        updatePrimes = false;
                        break;

                    case "-p":
                        if (args.Length >= i)
                        {
                            try
                            {
                                if (Directory.Exists(args[i++]))
                                {
                                    installDir = args[i];
                                    continue;
                                }
                            }
                            catch { }

                            Log.Print("'-p' must be followed by a valid path.");
                            return false;
                        }
                        else
                        {
                            Log.Print("'-p' must be followed by a valid path.");
                            return false;
                        }

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

            Directory.Delete(tmpDir, true);
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
