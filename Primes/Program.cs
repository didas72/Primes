using System;
using System.IO;
using System.Threading;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes.Exec
{
    public class Program
    {
        public static string homePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), resourcesPath, jobsPath, completePath;
        public static bool resourcesLoaded = false;

        private static volatile bool doWait = true;
        private static volatile bool isExiting = false;

        public static ulong[] knowPrimes;

        public static JobDistributer jobDistributer;



        public static void Main(string[] args)
        {
            if (!Init(ref args))
            {
                FailedInit();

                return;
            }

            LogExtension.LogEvent(Log.EventType.Info, "Initialization complete.", "MainThread", false);

            InitUI();

            Thread.Sleep(2000); //allow time for UI to init

            try
            {
                if (!Utils.HasDoableJobs(jobsPath))
                {
                    LogExtension.LogEvent(Log.EventType.Warning, "No jobs to be executed...", "MainThread", true);
                    Thread.Sleep(Properties.Settings.Default.FrameTimeMilis + 20);
                    Exit(true);
                }
                else
                    StartWork();
            }
            catch (Exception e)
            {
                LogExtension.LogEvent(Log.EventType.Error, $"Failed to start job execution: {e.Message}", "MainThread", false);
                LogExtension.LogEvent(Log.EventType.Error, "Failed to start job execution.", "MainThread", true, false);
            }

            while (doWait)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKey key = Console.ReadKey().Key;

                    if (key == ConsoleKey.Enter)
                        break;
                }
                
                Thread.Sleep(100);
            }

            if (doWait)
                Exit(true);
        }



        private static bool Init(ref string[] args)
        {
            try
            {
                SafeExit.Setup();

                if (!ParseArguments(ref args))
                    return false;

                if (!InitDirectories())
                    return false;

                LogExtension.InitLog(homePath);

                LogExtension.LogEvent(Log.EventType.Info, "Directories initialized.", "MainThread", true);

                if (!LoadResources())
                    return false;

                LogExtension.LogEvent(Log.EventType.Info, "Resources loaded.", "MainThread", true);

                if (!resourcesLoaded)
                    LogExtension.LogEvent(Log.EventType.Warning, "Some resource files were found. This could make prime search significantly slower.", "MainThread", true);

                if (!InitDistributer())
                    return false;

                LogExtension.LogEvent(Log.EventType.Info, "Distributer initialized.", "MainThread", true);

                return true;
            }
            catch (Exception e)
            {
                LogExtension.LogEvent(Log.EventType.Fatal, $"Error during initialization: {e.Message}", "MainThread", false);

                return false;
            }
        }
        private static void FailedInit()
        {
            LogExtension.LogEvent(Log.EventType.Fatal, "Failed to initialize! (If used the /? argument everything is ok)", "MainThread", false);

            LogExtension.Print("Press any key to exit.");

            Utils.WaitForKey();
        }
        private static bool ParseArguments(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "/?" || args[i] == "-?")
                {
                    LogExtension.Print("Arguments:");
                    LogExtension.Print("'-t T' - Number of threads to use, T being the desired value. T must be a integer, positive and non zero value.");
                    LogExtension.Print("'-p P' - Path of the directory to be used by this program, P being the desired path. P must be the FULL path to an existing directory.");
                    LogExtension.Print("'-b B' - Size (*8 bytes per thread) of the buffer that holds primes before adding to PrimeJobs. Lightly boosts performance but requires some extra memory.");
                    LogExtension.Print("'-q Q' - Maximum number of jobs paths queued for execution. Lower values result in slightly lower RAM usages and faster start-up times but more frequent scans for jobs.");
                    LogExtension.Print("'-f F' - Time, in milliseconds, between frames. Lower values will result in possibly faster refresh rates on the console but will slow prime calculations down slightly.");
                    LogExtension.Print("'-u U' - Wether or not UI is to be used, X representing a no and anything else a yes.");
                    LogExtension.Print("NOTE: Paths passed as arguments can only include spaces if encolsed in double quotes. Example: \"C:\\Documents\\primes\\\"");
                    return false;
                }
                else if (args[i] == "-t")
                {
                    if (args.Length >= i)
                    {
                        if (ushort.TryParse(args[i + 1], out ushort tArg))
                        {
                            if (tArg > 0)
                            {
                                Properties.Settings.Default.Threads = tArg;
                                Properties.Settings.Default.Save();
                            }
                            else
                            {
                                LogExtension.Print("Argument '-t' must be followed by a integer, positive and non zero value.");
                                return false;
                            }
                        }
                        else
                        {
                            LogExtension.Print("Argument '-t' must be followed by a integer, positive and non zero value.");
                            return false;
                        }
                    }
                    else
                    {
                        LogExtension.Print("Argument '-t' must be followed by a integer, positive and non zero value.");
                        return false;
                    }
                }
                else if (args[i] == "-p")
                {
                    if (args.Length >= i)
                    {
                        if (Directory.Exists(args[i + 1]))
                        {
                            Properties.Settings.Default.homePath = args[i + 1];
                            Properties.Settings.Default.Save();
                        }
                        else
                        {
                            LogExtension.Print($"Argument {args[i + 1]} is not a valid path.");
                            return false;
                        }
                    }
                    else
                    {
                        LogExtension.Print("Argument '-p' must be followed by a valid path.");
                        return false;
                    }
                }
                else if (args[i] == "-b")
                {
                    if (args.Length >= i)
                    {
                        if (uint.TryParse(args[i + 1], out uint bArg))
                        {
                            Properties.Settings.Default.PrimeBufferSize = bArg;
                            Properties.Settings.Default.Save();
                        }
                        else
                        {
                            LogExtension.Print("Argument '-b' must be followed by a integer, positive and non zero value.");
                            return false;
                        }
                    }
                    else
                    {
                        LogExtension.Print("Argument '-b' must be followed by a integer, positive and non zero value.");
                        return false;
                    }
                }
                else if (args[i] == "-q")
                {
                    if (args.Length >= i)
                    {
                        if (uint.TryParse(args[i + 1], out uint qArg))
                        {
                            Properties.Settings.Default.MaxJobQueue = qArg;
                            Properties.Settings.Default.Save();
                        }
                        else
                        {
                            LogExtension.Print("Argument '-q' must be followed by a integer, positive and non zero value.");
                            return false;
                        }
                    }
                    else
                    {
                        LogExtension.Print("Argument '-q' must be followed by a integer, positive and non zero value.");
                        return false;
                    }
                }
                else if (args[i] == "-f")
                {
                    if (args.Length >= i)
                    {
                        if (int.TryParse(args[i + 1], out int fArg))
                        {
                            Properties.Settings.Default.FrameTimeMilis = fArg;
                            Properties.Settings.Default.Save();
                        }
                        else
                        {
                            LogExtension.Print("Argument '-f' must be followed by a integer, positive and non zero value.");
                            return false;
                        }
                    }
                    else
                    {
                        LogExtension.Print("Argument '-f' must be followed by a integer, positive and non zero value.");
                        return false;
                    }
                }
                else if (args[i] == "-u")
                {
                    if (args.Length >= i)
                    {
                        if (args[i + 1].ToLowerInvariant() == "x")
                            Properties.Settings.Default.UseUI = false;
                        else
                            Properties.Settings.Default.UseUI = true;
                    }
                    else
                    {
                        LogExtension.Print("Argument '-u' must be followed by another value.");
                        return false;
                    }
                }
            }

            return true;
        }
        private static bool InitDirectories()
        {
            try
            {
                if (string.IsNullOrEmpty(Properties.Settings.Default.homePath))
                {
                    homePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
                    Directory.CreateDirectory(homePath);
                }
                else
                {
                    homePath = Properties.Settings.Default.homePath;
                    Directory.CreateDirectory(homePath);
                }

                resourcesPath = Path.Combine(homePath, "resources");
                Directory.CreateDirectory(resourcesPath);

                jobsPath = Path.Combine(homePath, "jobs");
                Directory.CreateDirectory(jobsPath);

                completePath = Path.Combine(homePath, "complete");
                Directory.CreateDirectory(completePath);
            }
            catch (Exception e)
            {
                LogExtension.LogEvent(Log.EventType.Error, $"Failed to initialize direcotries: '{e.Message}'", "MainThread", false);

                return false;
            }

            return true;
        }
        private static bool LoadResources()
        {
            string knwonPrimesFilePath = Path.Combine(resourcesPath, "knownPrimes.rsrc");

            try
            {
                if (!File.Exists(knwonPrimesFilePath))
                {
                    LogExtension.LogEvent(Log.EventType.Warning, "No knwon primes resource file found, skipping.", "ResourceLoading", false);

                    resourcesLoaded = false;

                    return true;
                }
            }
            catch (Exception e)
            {
                LogExtension.LogEvent(Log.EventType.Error, e.Message, "ResourceLoading", false);
            }

            try
            {
                KnownPrimesResourceFile file = KnownPrimesResourceFile.Deserialize(knwonPrimesFilePath);

                knowPrimes = file.Primes;

                resourcesLoaded = true;
            }
            catch (Exception e)
            {
                LogExtension.LogEvent(Log.EventType.Warning, $"Failed to load knwon primes resource file: {e.Message}; {e.StackTrace}", "MainThread", false);

                resourcesLoaded = false;
            }

            return true;
        }
        private static bool InitDistributer()
        {
            jobDistributer = new JobDistributer(Properties.Settings.Default.Threads, jobsPath, completePath);

            return true;
        }
        private static void InitUI()
        {
            if (Properties.Settings.Default.UseUI)
                ConsoleUI.StartUI();
            else
                LogExtension.InitConsole();
        }



        public static void Exit(bool waitForUser)
        {
            if (isExiting)
                return;

            isExiting = true;

            LogExtension.LogEvent(Log.EventType.Info, "Preparng to exit...", "MainThread", true);

            doWait = false;

            StopWork();

            jobDistributer.WaitForAllWorkers();

            if (ConsoleUI.UIEnabled)
                ConsoleUI.StopUI();

            LogExtension.LogEvent(Log.EventType.Info, "Exiting.", "MainThread", false);

            if (waitForUser)
            {
                LogExtension.Print("Press any key to exit...");

                Utils.WaitForKey();
            }

            Environment.Exit(0);
        }



        private static void StartWork()
        {
            LogExtension.LogEvent(Log.EventType.Info, "Starting work...", "MainThread", true);

            jobDistributer.StartWork();
        }
        private static void StopWork()
        {
            LogExtension.LogEvent(Log.EventType.Info, "Stopping work...", "MainThread", true);

            jobDistributer.StopWork();
        }
    }
}
