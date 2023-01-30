using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

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

            //TODO: Change execution loop to instead join the control listener when done
            while (true)
            {
                if (Console.KeyAvailable) break;
                Thread.Sleep(10);
            }

            Log.LogEvent("Server stopping...", "Main");
            StopServer();
            Log.LogEvent("Done.", "Main");
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
                if (!ParseArgs(args)) return false;
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
                string setsPath = Path.Combine(Environment.CurrentDirectory, "settings.set");

                if (File.Exists(setsPath))
                    Globals.settings = SettingsDocument.Deserialize(Path.Combine(Environment.CurrentDirectory, "settings.set"));
                else
                    Globals.settings = new SettingsDocument();

                Dictionary<string, string> scheme = new()
                {
                    { "homeDir", Environment.CurrentDirectory },
                    { "maxAssignedBatches", "5" },
                    { "maxDesyncOps", "10" },
                    { "batchExpireHours", "120" }, //5 days
                    { "clientExpireHours", "2160" }, //3 months
                    { "clientPort", "13032" },
                    { "controlPort", "13033" },
                    { "forceLoadCliDta", bool.FalseString }
                };

                Globals.settings.ApplySettingsScheme(scheme, false);

                SettingsDocument.Serialize(Globals.settings, setsPath);
            }
            catch (Exception e)
            {
                Log.LogException("Failed to init settings.", "IntitSettings", e);
                return false;
            }

            return true;
        }
        private static bool ParseArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "/?":
                    case "-?":
                    case "help":
                    case "--help":
                        Console.WriteLine("BatchServer available arguments:" +
                            "'/?' or '-?' or 'help' or '--help' - Display this message." +
                            "'-f' - Force loading of Client Data, regardless of existence of .lock file." +
                            "'-x' - Ensures the existence of a valid settings file and exits. Useful to configure the server for the first time.");
                        Environment.Exit(0);
                        return false; //not actually needed but clean

                    case "-x":
                        Console.WriteLine("Settings saved.");
                        Environment.Exit(0);
                        return false;

                    case "-f":
                        Globals.settings.SetValue("forceLoadCliDta", true);
                        break;

                    default:
                        Log.LogEvent(Log.EventType.Warning, $"Unknown argument '{args[i]}'. Ignoring.", "ParseArgs");
                        break;
                }
            }

            return true;
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

                Globals.clientData = new(filePath, Globals.settings.GetBool("forceLoadCliDta"), Globals.settings.GetInt("maxAssignedBatches"), Globals.settings.GetInt("maxDesyncOps"), TimeSpan.FromHours(Globals.settings.GetInt("batchExpireHours")), TimeSpan.FromHours(Globals.settings.GetInt("clientExpireHours")));
                Globals.clientData.OnTimedActions(null, null);

                Globals.ExpireElementsTimer = new()
                {
                    AutoReset = true,
                    Interval = 1000 * 60 * 60 * 3, //every 3 hours
                };

                Globals.ExpireElementsTimer.Elapsed += Globals.clientData.OnTimedActions;
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
            try { Globals.clientListener?.Stop(); } catch (Exception e) { Log.LogException("Failed to stop client listener.", "StopServer", e); }
            try { Globals.clientData?.Close(); } catch (Exception e) { Log.LogException("Failed to close client data.", "StopServer", e); }
            try { SettingsDocument.Serialize(Globals.settings, Path.Combine(Environment.CurrentDirectory, "settings.set")); } catch (Exception e) { Log.LogException("Failed to save settings.", "StopServer", e); }
        }
    }
}