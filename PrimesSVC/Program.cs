using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using DidasUtils.Logging;

namespace Primes.SVC
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (!Init(args))
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to init.", "Main");
                Environment.Exit(1);
            }

            Log.LogEvent("Service started.", "Main");

            try
            {
                ControlListener.ListenAndJoin();
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Fatal, "Unhandled exception, see below.", "Main");
                Log.LogException("Fatal exception.", "Main", e);
                Environment.Exit(1);
                return; //sanity and proper branch flow for VS
            }

            //regular exit
            Log.LogEvent("Service stopping...", "Main");

            WorkCoordinator.StopWork();
            System.Threading.Thread.Sleep(300); //just to be seen while testing
            Environment.Exit(0);
            return; //sanity and proper branch flow for VS
        }



        private static bool Init(string[] args)
        {
            try
            {
                Log.UsePrint = false;
                Globals.startLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
                Log.InitLog(Globals.startLogPath, "SVC_start_log.txt");
                Globals.startLogPath = Path.Combine(Globals.startLogPath, "SVC_start_log.txt");

                GetOS();

                //TODO: parse args

                if (!InitSettings()) return false;
                if (!InitDirs()) return false;
                if (!InitLog()) return false;
                if (!ControlListener.Init()) return false;
                if (!ResourceHolder.Init()) return false;
                if (!BatchManager.Init()) return false;
                if (!Scheduler.Init()) return false;
                if (!WorkCoordinator.Init()) return false;
                if (!WorkCoordinator.InitWorkers()) return false;
            }
            catch (Exception e)
            {
                Log.LogException("Failed to init.", "Init", e);
                return false;
            }

            return true;
        }
        private static void GetOS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Globals.currentOS = OS.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Globals.currentOS = OS.Linux;
                throw new NotImplementedException("PrimesSVC does not support Linux.");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Globals.currentOS = OS.OSX;
                throw new NotImplementedException("PrimesSVC does not support OSX.");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                Globals.currentOS = OS.FreeBSD;
                throw new NotImplementedException("PrimesSVC does not support FreeBSD.");
            }
        }
        private static bool InitSettings()
        {
            switch (Globals.currentOS)
            {
                case OS.Windows:
                    if (!Settings.InitSettings_WinReg()) return false;
                    break;

                default:
                    Log.LogEvent(Log.EventType.Error, $"OS {Globals.currentOS} is not supported or is not a valid option.", "InitSettings");
                    return false;
            }

            return true;
        }
        private static bool InitDirs()
        {
            try
            {
                Globals.homeDir = Settings.HomeDir;
                Directory.CreateDirectory(Globals.homeDir);

                Globals.resourcesDir = Path.Combine(Globals.homeDir, "resources");
                Directory.CreateDirectory(Globals.resourcesDir);
                Globals.jobsDir = Path.Combine(Globals.homeDir, "jobs");
                Directory.CreateDirectory(Globals.jobsDir);
                Globals.completeDir = Path.Combine(Globals.homeDir, "complete");
                Directory.CreateDirectory(Globals.completeDir);

                //clear cache
                Globals.cacheDir = Path.Combine(Globals.homeDir, "cache");
                Directory.Delete(Globals.cacheDir, true);
                Directory.CreateDirectory(Globals.cacheDir);
            }
            catch (Exception e)
            {
                Log.LogException("Failed to init dirs.", "InitDirs", e);
                return false;
            }

            return true;
        }
        private static bool InitLog()
        {
            Log.InitLog(Path.Combine(Globals.homeDir), "SVC_log.txt");
            Log.UsePrint = false;

            try { File.Delete(Path.Combine(Globals.startLogPath)); } catch { }

            return true;
        }
    }
}