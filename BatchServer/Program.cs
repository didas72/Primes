﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Files;

namespace BatchServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Init();

            //code

            Exit();
        }



        static void Init()
        {
            InitDirs();

            InitLog();

            InitSettings();

            InitDB();
        }

        static void InitDirs()
        {
            //to run in raspberry / linux

            //create data path if needed
            Directory.CreateDirectory("/var/lib/didas72/");
            Directory.CreateDirectory("/var/lib/didas72/batchServer/");

            Globals.dataPath = "/var/lib/didas72/batchServer/";

            Directory.CreateDirectory("/var/log/didas72/");
            Directory.CreateDirectory("/var/log/didas72/batchServer/");

            Globals.logPath = "/var/log/didas72/batchServer/";

            Directory.CreateDirectory("/etc/didas72/");
            Directory.CreateDirectory("/etc/didas72/batchServer/");

            Globals.settingsPath = "/etc/didas72/batchServer/";
        }

        static void InitLog()
        {
            Log.InitLog(Globals.logPath, "main.log");
            Log.LogEvent($"OS: {Environment.OSVersion.VersionString}", "Init");
        }

        static void InitSettings()
        {
            if (File.Exists(Globals.settingsPath + "settings.set"))
            {
                Globals.settings = new SettingsDocument(File.ReadAllText(Globals.settingsPath + "settings.set"));
            }
            else
            {
                Log.LogEvent(Log.EventType.Warning, "No settings file present.", "InitSettings");

                Globals.settings = new SettingsDocument();
                Globals.settings.ApplySettingsScheme(new Dictionary<string, string>() { { "server", "localhost" }, { "user", "root" }, { "password", "" } }, false);
            }
        }

        static void InitDB()
        {
            Globals.Db = new DbWrapper();

            Globals.Db.Connect(Globals.settings.GetValue("server"), Globals.settings.GetValue("user"), Globals.settings.GetValue("password"));

            int reply;

            reply = Globals.Db.SendCommandNonQuery("CREATE DATABASE IF NOT EXISTS batchServer;");
            if (reply != 0) Log.LogEvent("Database 'batchServer' did not exists and was created.", "InitDB");

            Globals.Db.SendCommandNonQuery("USE batchServer;");

            Globals.Db.SendCommandNonQuery("CREATE TABLE IF NOT EXISTS users(user_id INT AUTO_INCREMENT PRIMARY KEY, last_ip INT, last_contacted DATETIME, last_requested DATETIME, last_submitted DATETIME);");
            Globals.Db.SendCommandNonQuery("CREATE TABLE IF NOT EXISTS jobs(job_start BIGINT UNIQUE NOT NULL PRIMARY KEY, status TINYINT DEFAULT 0, added DATETIME NOT NULL, assigned_user INT, last_sent DATETIME, last_received DATETIME);");
        }



        static void Exit()
        {
            Globals.Db.Dispose();

            Log.LogEvent("Exiting.", "Main");
        }
    }
}
