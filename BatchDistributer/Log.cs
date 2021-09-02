using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes.BatchDistributer
{
    public static class Log
    {
        private static string logPath;


        public enum EventType : byte
        {
            Info,
            Warning,
            HighWarning,
            Error,
            Fatal,
            Performance
        }



        public static void InitLog(string path)
        {
            logPath = path;

            DateTime now = DateTime.Now;
            string log = $@"

===============================================
Start time {now.Hour}:{now.Minute}:{now.Second}
===============================================


";
            try
            {
                File.AppendAllText(Path.Combine(logPath, "log.txt"), log);
            }
            catch
            {
                SafeWriteLine("Failed to write log to file.");
            }
        }
        public static void InitConsole()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void LogEvent(string msg, string sender) => LogEvent(EventType.Info, msg, sender, true, true);
        public static void LogEvent(EventType eventType, string msg, string sender) => LogEvent(eventType, msg, sender, true, true);
        public static void LogEvent(EventType eventType, string msg, string sender, bool writeToScreen) => LogEvent(eventType, msg, sender, writeToScreen, true);
        public static void LogEvent(EventType eventType, string msg, string sender, bool writeToScreen, bool writeToFile)
        {
            DateTime now = DateTime.Now;

            string log = $"[{now.Hour:00}:{now.Minute:00}:{now.Second:00}] {sender}: [{eventType}] {msg}";

            if (writeToScreen)
            {
                switch (eventType)
                {
                    case EventType.Info:
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        break;

                    case EventType.Performance:
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        break;

                    case EventType.Warning:
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case EventType.HighWarning:
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;

                    case EventType.Error:
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;

                    case EventType.Fatal:
                        Console.BackgroundColor = ConsoleColor.DarkRed;
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                }

                SafeWriteLine(log);
            }

            if (logPath != null && writeToFile)
                if (!TryWriteLog(Path.Combine(logPath, "log.txt"), log + "\n"))
                    Print("Failed to write log to file.");
        }
        public static void Print(string msg)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            SafeWriteLine(msg);
        }



        public static void SafeWriteLine(string msg)
        {
            lock (ConsoleLock.console)
            {
                Console.WriteLine(msg);
            }
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
                Thread.Sleep(5);
            }

            return false;
        }
    }
}
