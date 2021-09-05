﻿using System;
using System.IO;
using System.Threading;

namespace Primes.Common
{
    /// <summary>
    /// Class containing methods to log events to a file and/or print them to console.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// The path to the log file.
        /// </summary>
        public static string LogPath { get; private set; }



        /// <summary>
        /// Enum that specifies the accepted event types.
        /// </summary>
        public enum EventType : byte
        {
            /// <summary>
            /// Informative only event. Not a concern to the user nor to the programmer.
            /// </summary>
            Info,
            /// <summary>
            /// Potentially problematic event. Not much of a concern to the user but something for the programmer to think about.
            /// </summary>
            Warning,
            /// <summary>
            /// A potentially problematic even, possibly leading to a bigger probelm. A concern to both the user and the programmer.
            /// </summary>
            HighWarning,
            /// <summary>
            /// An exception type event. A concern to both the user and the programmer.
            /// </summary>
            Error,
            /// <summary>
            /// A catastrophic event, definitely leading to a crash or controlled crash.
            /// </summary>
            Fatal,
            /// <summary>
            /// Informative only event, reporting program performance. Useful for both the user and the programmer.
            /// </summary>
            Performance
        }



        /// <summary>
        /// Initializes the log file. 
        /// </summary>
        /// <param name="path">The path to the log file.</param>
        /// <remarks>Must be called once before calling any other log related method.</remarks>
        public static void InitLog(string path)
        {
            LogPath = path;

            DateTime now = DateTime.Now;
            string log = $@"

===============================================
Start time {now.Hour}:{now.Minute}:{now.Second}
===============================================


";
            try
            {
                File.AppendAllText(Path.Combine(LogPath, "log.txt"), log);
            }
            catch
            {
                SafeWriteLine("Failed to write log to file.");
            }
        }
        /// <summary>
        /// Initializes console for use with this class.
        /// </summary>
        public static void InitConsole()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }



        /// <summary>
        /// Logs an event of type EventType.Info, printing to screen and saving to file.
        /// </summary>
        /// <param name="msg">The log message.</param>
        /// <param name="sender">The name of the sender.</param>
        public static void LogEvent(string msg, string sender) => LogEvent(EventType.Info, msg, sender, true, true);
        /// <summary>
        /// Logs an event, printing to screen and saving to file.
        /// </summary>
        /// <param name="eventType">The type of event to be logged.</param>
        /// <param name="msg">The log message.</param>
        /// <param name="sender">The name of the sender.</param>
        public static void LogEvent(EventType eventType, string msg, string sender) => LogEvent(eventType, msg, sender, true, true);
        /// <summary>
        /// Logs an event, optionally printing it to screen.
        /// </summary>
        /// <param name="eventType">The type of event to be logged.</param>
        /// <param name="msg">The log message.</param>
        /// <param name="sender">The name of the sender.</param>
        /// <param name="writeToScreen">>Wether or not to print to screen.</param>
        public static void LogEvent(EventType eventType, string msg, string sender, bool writeToScreen) => LogEvent(eventType, msg, sender, writeToScreen, true);
        /// <summary>
        /// Logs an event, optionally printing it to screen and saving to file.
        /// </summary>
        /// <param name="eventType">The type of event to be logged.</param>
        /// <param name="msg">The log message.</param>
        /// <param name="sender">The name of the sender.</param>
        /// <param name="writeToScreen">Wether or not to print to screen.</param>
        /// <param name="writeToFile">Wether or not to save to file.</param>
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

            if (LogPath != null && writeToFile)
                if (!TryWriteLog(Path.Combine(LogPath, "log.txt"), log + "\n"))
                    Print("Failed to write log to file.");
        }



        /// <summary>
        /// Prints a message to screen without any other log information.
        /// </summary>
        /// <param name="msg">The message to print to screen.</param>
        public static void Print(string msg)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            SafeWriteLine(msg);
        }



        /// <summary>
        /// Writes a line to console in a thread safe way.
        /// </summary>
        /// <param name="msg">The message to print to screen.</param>
        public static void SafeWriteLine(string msg)
        {
            lock (ConsoleLock.console)
            {
                Console.WriteLine(msg);
            }
        }
        /// <summary>
        /// Tries to write log to file, attempting up to ten times.
        /// </summary>
        /// <param name="path">The path to the log file.</param>
        /// <param name="log">The log to write to file.</param>
        /// <returns>Boolean indicating the operation's success.</returns>
        public static bool TryWriteLog(string path, string log)
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
