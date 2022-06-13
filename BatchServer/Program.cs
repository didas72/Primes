using System;
using System.IO;
using System.Collections.Generic;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Files;

namespace BatchServer
{
    internal static class Program
    {
        //TODO: Control listener

        private static void Main(string[] args)
        {
            if (!Init(args))
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to init.", "Main");
                Environment.Exit(1);
            }

            Log.LogEvent("Server started.", "Main");

            //TODO: execution loop, probably will join the control listener or something similar

            Log.LogEvent("Server stopping...", "Main");
            StopServer();
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

                if (!InitSettings()) return false;
                if (!InitDirs()) return false;
                if (!InitLog()) return false;
                if (!InitClientData()) return false;
                if (!InitClientListener()) return false;
            }
            catch (Exception e)
            {
                Log.LogException("Failed to init.", "Init", e);
                return false;
            }

            return true;
        }
        private static bool InitSettings()
        {
            try
            {
                Globals.settings = SettingsDocument.Deserialize(Path.Combine(Environment.CurrentDirectory, "settings.set"));

                Dictionary<string, string> scheme = new()
                {
                    { "homeDir", Environment.CurrentDirectory },
                    { "maxAssignedBatches", "5" },
                    { "maxDesyncOps", "10" },
                    { "batchExpireHours", "120" }, //5 days
                    { "clientExpireHours", "720" }, //1 month
                    { "clientPort", "13032" },
                    { "controlPort", "13033" }
                };

                Globals.settings.ApplySettingsScheme(scheme, false);
            }
            catch (Exception e)
            {
                Log.LogException("Failed to init settings.", "IntitSettings", e);
                return false;
            }

            return false;
        }
        private static bool InitDirs()
        {
            try
            {
                Globals.homeDir = Globals.settings.GetString("homeDir");
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
            Log.InitLog(Path.Combine(Globals.homeDir), "Server_log.txt");
            Log.UsePrint = false;

            try { File.Delete(Path.Combine(Globals.startLogPath)); } catch { }

            return true;
        }
        private static bool InitClientData()
        {
            try
            {
                string filePath = Path.Combine(Globals.homeDir, "cliDta.dta");
                Globals.clientData = new(filePath, false, Globals.settings.GetInt("maxAssignedBatches"), Globals.settings.GetInt("maxDesyncOps"), TimeSpan.FromHours(Globals.settings.GetInt("batchExpireHours")), TimeSpan.FromHours(Globals.settings.GetInt("clientExpireHours")));
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
        private static bool InitClientListener()
        {
            Globals.clientListener = new(Globals.settings.GetInt("clientPort"));
            Globals.clientListener.Start();

            return true;
        }



        private static void StopServer()
        {
            Globals.clientListener?.Stop();
            Globals.clientData?.Close();
        }
    }
}