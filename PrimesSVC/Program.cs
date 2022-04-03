using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

using DidasUtils.Logging;

using Primes.Common;

namespace Primes.SVC
{
    class Program
    {
        private static void Main(string[] args)
        {
            if (!Init())
                Log.LogEvent(Log.EventType.Fatal, "Failed to init.", "Main");
        }



        private static bool Init()
        {
            Log.UsePrint = false;
            Globals.startLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
            Log.InitLog(Globals.startLogPath, "SVC_start_log.txt");
            Globals.startLogPath = Path.Combine(Globals.startLogPath, "SVC_start_log.txt");

            GetOS();

            if (!InitSettings())
                return false;

            if (!InitDirs())
                return false;

            if (!InitLog())
                return false;

            if (!ControlListener.Init())
                return false;

            if (!ResourceHolder.Init())
                return false;
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
                Directory.CreateDirectory(Globals.homeDir);

                Globals.resourcesDir = Path.Combine(Globals.homeDir, "resources");
                Directory.CreateDirectory(Globals.resourcesDir);
                Globals.jobsDir = Path.Combine(Globals.homeDir, "jobs");
                Directory.CreateDirectory(Globals.jobsDir);
                Globals.completeDir = Path.Combine(Globals.homeDir, "complete");
                Directory.CreateDirectory(Globals.completeDir);
                Globals.cacheDir = Path.Combine(Globals.homeDir, "cache");
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

            try { File.Delete(Path.Combine(Globals.startLogPath)); } catch { }

            return true;
        }
    }
}