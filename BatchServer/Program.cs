using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Files;

using BatchServer.Modules;

namespace BatchServer
{
    /*
     TODO:
        - Not close right away
        + Add icon
        - Add controls to:
            - add new batches
            - fetch finished batches
            - control batch info
            - stop server
            - restart server
            - get user info
            - control user info
        - Add handling of requests
        - Add checking the time of last time up, if long elapsed, extend all user return to today += ?2?
        - Add missing logging for connects and interacts
        +-Add specifiy of batch store paths
        + Test!
        + ---Add missing messages---
     */

    class Program
    {
        private static volatile bool running = false;

        static void Main(string[] args)
        {
            if (!Init()) Exit();

            running = false;

            while (running)
            {
                Thread.Sleep(50);
            }

            Exit();
        }



        static bool Init()
        {
            Log.InitConsole();

            if (!InitDirs())
            {
                Log.Print("Failed to init directories!");
                return false;
            }

            if (!InitLog())
            {
                Log.Print("Failed to init log!");
                return false;
            }

            if (!InitSettings())
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to init settings!", "Init");
                return false;
            }

            if (!InitDB())
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to init database!", "Init");
                return false;
            }

            if (!InitNet())
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to init networking!", "Init");
                return false;
            }

            Log.LogEvent("Initialization complete.", "Init");

            return true;
        }

        static bool InitDirs()
        {
            //to run in raspberry / linux

            //create data path if needed
            try
            {
                Directory.CreateDirectory("/var/lib/didas72/");
                Directory.CreateDirectory("/var/lib/didas72/batchServer/");

                Globals.dataPath = "/var/lib/didas72/batchServer/";

                Directory.CreateDirectory("/var/log/didas72/");
                Directory.CreateDirectory("/var/log/didas72/batchServer/");

                Globals.logPath = "/var/log/didas72/batchServer/";

                Directory.CreateDirectory("/etc/didas72/");
                Directory.CreateDirectory("/etc/didas72/batchServer/");

                Globals.settingsPath = "/etc/didas72/batchServer/";

                if (!Directory.Exists("/media/usb/")) return false;

                Globals.batchesPath = "/media/usb/";
            }
            catch (Exception e)
            {
                Log.Print($"Error initializing directories: {e}");

                return false;
            }

            return true;
        }

        static bool InitLog()
        {
            try
            {
                Log.InitLog(Globals.logPath, "main.log");
                Log.LogEvent($"OS: {Environment.OSVersion.VersionString}", "Init");
            }
            catch (Exception e)
            {
                Log.Print($"Failed to init log: {e}");
                return false;
            }
            return true;
        }

        static bool InitSettings()
        {
            if (File.Exists(Globals.settingsPath + "settings.set"))
            {
                Globals.settings = new(File.ReadAllText(Globals.settingsPath + "settings.set"));
            }
            else
            {
                Log.LogEvent(Log.EventType.Warning, "No settings file present.", "InitSettings");

                Globals.settings = new();
                Globals.settings.ApplySettingsScheme(new Dictionary<string, string>() { { "server", "localhost" }, { "user", "root" }, { "password", "" }, { "port", "5505"} }, false);
            }

            return true;
        }

        static bool InitDB()
        {
            try
            {
                Globals.Db = new DbWrapper();

                Globals.Db.Connect(Globals.settings.GetString("server"), Globals.settings.GetString("user"), Globals.settings.GetString("password"));

                int reply;

                reply = Globals.Db.SendCommandNonQuery("CREATE DATABASE IF NOT EXISTS batchServer;");
                if (reply != 0) Log.LogEvent("Database 'batchServer' did not exists and was created.", "InitDB");

                Globals.Db.SendCommandNonQuery("USE batchServer;");

                Globals.Db.SendCommandNonQuery("CREATE TABLE IF NOT EXISTS users(user_id INT AUTO_INCREMENT PRIMARY KEY, last_ip INT, last_contacted DATETIME, last_requested DATETIME, last_submitted DATETIME);");
                Globals.Db.SendCommandNonQuery("CREATE TABLE IF NOT EXISTS jobs(job_start BIGINT UNIQUE NOT NULL PRIMARY KEY, status TINYINT DEFAULT 0, added DATETIME NOT NULL, assigned_user INT, last_sent DATETIME, last_received DATETIME);");
            }
            catch (Exception e)
            {
                Log.LogException("Error initializing database.", "InitDB", e);
                return false;
            }

            return true;
        }

        static bool InitNet()
        {
            try
            {
                Globals.conListener = new();
                Globals.clHandle = new();
                Globals.ctlHandle = new();
                Globals.server = new();

                Globals.conListener.StartListener(Globals.settings.GetInt("port"));
            }
            catch (Exception e)
            {
                Log.LogException("Error initializing networking.", "InitNet", e);
                return false;
            }

            return true;
        }



        static void Exit()
        {
            Globals.Db.Dispose();

            Log.LogEvent("Exiting.", "Main");
        }
    }
}
