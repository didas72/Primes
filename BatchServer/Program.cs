using System;
using System.IO;
using System.Collections.Generic;

using DidasUtils;
using DidasUtils.Logging;

namespace BatchServer
{
    internal static class Program
    {
        //TODO: Control listener
        //TODO: Timer to expire batches and users

        private static void Main(string[] args)
        {
            if (!Init(args))
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to init.", "Main");
                Environment.Exit(1);
            }

            Log.LogEvent("Server started.", "Main");

            //TODO: execution loop

            Log.LogEvent("Server stopping...", "Main");
            Environment.Exit(0);
            return; //sanity and proper branch flow for VS
        }



        private static bool Init(string[] args)
        {
            try
            {
                Log.UsePrint = false;
                Globals.startLogPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                Log.InitLog(Globals.startLogPath, "BatchServer_start_log.txt");
                Globals.startLogPath = Path.Combine(Globals.startLogPath, "BatchServer_start_log.txt");

                if (!InitDirs()) return false;
                if (!InitLog()) return false;
                if (!InitClientData()) return false;
            }
            catch (Exception e)
            {
                Log.LogException("Failed to init.", "Init", e);
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

                Globals.sourceDir = Path.Combine(Globals.homeDir, "source");
                Directory.CreateDirectory(Globals.sourceDir);
                Globals.completeDir = Path.Combine(Globals.homeDir, "complete");
                Directory.CreateDirectory(Globals.completeDir);

                //clear cache
                Globals.cacheDir = Path.Combine(Globals.homeDir, "cache");
                if (Directory.Exists(Globals.cacheDir))
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
        private static bool InitClientData()
        {
            try
            {
                string filePath = Path.Combine(Globals.homeDir, "cliDta.dta");
                Globals.clientData = new(filePath, false, 5, 5, TimeSpan.FromDays(5), TimeSpan.FromDays(120)); //TODO: Add settings here
                Globals.clientData.OnExpireElements(null, null);

                Globals.ExpireElementsTimer = new()
                {
                    AutoReset = true,
                    Interval = 1000 * 60 * 60 * 3, //every 3 hours
                };

                Globals.ExpireElementsTimer.Elapsed += Globals.clientData.OnExpireElements;
                Globals.ExpireElementsTimer.Start();
            }
            catch (Exception e)
            {
                Log.LogException("Failed to init client data.", "InitClientData", e);
                return false;
            }

            return true;
        }
    }
}