﻿using System;
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
                LogEvent(EventType.Fatal, "Failed to initialize!", "MainThread", false);

                Print("Press any key to exit.");

                Console.ReadKey(true);

                return;
            }

            LogEvent(EventType.Info, "Initialization complete.", "MainThread", false);

            if (!Utils.HasDoableJobs(jobsPath))
            {
                LogEvent(EventType.Error, "There are no pending jobs. Please add some before starting the prime search.", "MainThread", true);
                Print("Press any key to exit.");

                Console.ReadKey();

                Exit(false);
            }

            if (!resourcesLoaded)
            {
                LogEvent(EventType.Warning, "Not all resources files were found. This will make prime search significantly slower.", "MainThread", true);
            }

            StartWork();

            ConsoleUI.StartUI();

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
            SafeExit.Setup();

            if (!ParseArguments(ref args))
                return false;

            if (!InitDirectories())
                return false;

            InitLog();

            LogEvent(EventType.Info, "Directories initialized.", "MainThread", true);

            if (!LoadResources())
                return false;

            LogEvent(EventType.Info, "Resources loaded.", "MainThread", true);

            if (!InitDistributer())
                return false;

            LogEvent(EventType.Info, "Distributer initialized.", "MainThread", true);

            return true;
        }
        private static bool ParseArguments(ref string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "/?")
                {
                    Print("Arguments:");
                    Print("'-t T' - Number of threads to use, T being the desired value. T must be a integer, positive and non zero value.");
                    Print("'-p P' - Path of the directory to be used by this program, P being the desired path. P must be the FULL path to an existing directory.");
                    Print("'-b B' - Size (*8 bytes per thread) of the buffer that holds primes before adding to PrimeJobs. Lightly boosts performance but requires some extra memory.");
                    Print("'-q Q' - Maximum number of jobs paths queued for execution. Lower values result in slightly lower RAM usages and faster start-up times but more frequent scans for jobs.");
                    Print("'-f F' - Time, in milliseconds, between frames. Lower values will result in possibly faster refresh rates on the console but will slow prime calculations down slightly.");
                    Print("NOTE: Paths passed as arguments can only include spaces if encolsed in double quotes. Example: \"C:\\Documents\\primes\\\"");
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
                                Print("Argument '-t' must be followed by a integer, positive and non zero value.");
                                return false;
                            }
                        }
                    }
                    else
                    {
                        Print("Argument '-t' must be followed by a integer, positive and non zero value.");
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
                            Print($"Argument {args[i + 1]} is not a valid path.");
                            return false;
                        }
                    }
                    else
                    {
                        Print("Argument '-p' must be followed by a valid path.");
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
                    }
                    else
                    {
                        Print("Argument '-b' must be followed by a integer, positive and non zero value.");
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
                    }
                    else
                    {
                        Print("Argument '-q' must be followed by a integer, positive and non zero value.");
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
                    }
                    else
                    {
                        Print("Argument '-f' must be followed by a integer, positive and non zero value.");
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
                LogEvent(EventType.Error, $"Failed to initialize direcotries: '{e.Message}'", "MainThread", false);

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
                    LogEvent(EventType.Warning, "No knwon primes resource file found, skipping.", "ResourceLoading", false);

                    resourcesLoaded = false;

                    return true;
                }
            }
            catch (Exception e)
            {
                LogEvent(EventType.Error, e.Message, "ResourceLoading", false);
            }

            try
            {
                KnownPrimesResourceFile file = KnownPrimesResourceFile.Deserialize(knwonPrimesFilePath);

                knowPrimes = file.Primes;

                resourcesLoaded = true;
            }
            catch (Exception e)
            {
                LogEvent(EventType.Warning, $"Failed to load knwon primes resource file: {e.Message}; {e.StackTrace}", "MainThread", false);

                resourcesLoaded = false;
            }

            return true;
        }
        private static bool InitDistributer()
        {
            jobDistributer = new JobDistributer(Properties.Settings.Default.Threads, jobsPath, completePath);

            return true;
        }



        public static void Exit(bool waitForUser)
        {
            if (isExiting)
                return;

            isExiting = true;

            LogEvent(EventType.Info, "Preparng to exit...", "MainThread", true);

            doWait = false;

            StopWork();

            jobDistributer.WaitForAllWorkers();

            ConsoleUI.StopUI();

            LogEvent(EventType.Info, "Exiting.", "MainThread", false);

            if (waitForUser)
            {
                Print("Press any key to exit...");

                Console.ReadKey();
            }

            Environment.Exit(0);
        }



        private static void StartWork()
        {
            LogEvent(EventType.Info, "Starting work...", "MainThread", true);

            jobDistributer.StartWork();
        }
        private static void StopWork()
        {
            LogEvent(EventType.Info, "Stopping work...", "MainThread", true);

            jobDistributer.StopWork();
        }



        public enum EventType : byte
        {
            Info,
            Warning,
            HighWarning,
            Error,
            Fatal,
            Performance
        }
        public static void InitLog()
        {
            DateTime now = DateTime.Now;
            string log = $@"

===============================================
Start time {now.Hour}:{now.Minute}:{now.Second}
===============================================


";
            try
            {
                File.AppendAllText(Path.Combine(homePath, "log.txt"), log);
            }
            catch
            {
                Console.WriteLine("Failed to write log to file.");
            }
        }
        public static void LogEvent(EventType eventType, string msg, string sender, bool writeToScreen)
        {
            DateTime now = DateTime.Now;

            string log = $"[{now.Hour:00}:{now.Minute:00}:{now.Second:00}] {sender}: [{eventType}] {msg}";

            if (writeToScreen)
                ConsoleUI.AddLog(log);

            if (!TryWriteLog(Path.Combine(homePath, "log.txt"), log + "\n"))
                ConsoleUI.AddLog("Failed to write log to file.");
        }
        private static bool TryWriteLog(string path, string log)
        {
            int triesLeft = 10;

            while (triesLeft > 0)
            {
                try
                {
                    File.AppendAllText(path, log);

                    return true;
                }
                catch { }

                triesLeft--;
                Thread.Sleep(0);
            }

            return false;
        }



        public static void Print(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
