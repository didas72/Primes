using System;
using System.IO;

using DidasUtils.Logging;

namespace Primes.Exec
{
    static class LogExtension
    {
        public static void InitLog(string path) => Log.InitLog(path);
        public static void InitConsole() => Log.InitConsole();
        public static void Print(string msg) => Log.Print(msg);


        public static void LogEvent(string msg, string sender) => LogEvent(Log.EventType.Info, msg, sender, true, true);
        public static void LogEvent(Log.EventType eventType, string msg, string sender) => LogEvent(eventType, msg, sender, true, true);
        public static void LogEvent(Log.EventType eventType, string msg, string sender, bool writeToScreen) => LogEvent(eventType, msg, sender, writeToScreen, true);
        public static void LogEvent(Log.EventType eventType, string msg, string sender, bool writeToScreen, bool writeToFile)
        {
            DateTime now = DateTime.Now;

            string log = $"[{now.Hour:00}:{now.Minute:00}:{now.Second:00}] {sender}: [{eventType}] {msg}";

            if (writeToScreen)
            {
                if (ConsoleUI.UIEnabled)
                    ConsoleUI.AddLog(log);
                else
                {
                    switch (eventType)
                    {
                        case Log.EventType.Info:
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            break;

                        case Log.EventType.Performance:
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            break;

                        case Log.EventType.Warning:
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;

                        case Log.EventType.HighWarning:
                            Console.BackgroundColor = ConsoleColor.Yellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                            break;

                        case Log.EventType.Error:
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            break;

                        case Log.EventType.Fatal:
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            Console.ForegroundColor = ConsoleColor.Black;
                            break;
                    }

                    Log.SafeWriteLine(log);
                }
            }

            if (Log.LogPath != null && writeToFile)
                if (!Log.TryWriteLog(Path.Combine(Log.LogPath, "log.txt"), log + "\n"))
                    Log.Print("Failed to write log to file.");
        }
    }
}
