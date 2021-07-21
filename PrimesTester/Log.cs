using System;
using System.IO;

namespace Primes.Tester
{
    class Log
    {
        private readonly string path;

        public Log(string path)
        {
            this.path = path;
        }

        public void Write(EventType eventType, string message)
        {
            DateTime now = DateTime.Now;

            File.AppendAllText(path, $"[{now.Hour:00}:{now.Minute:00}:{now.Second:00}] ({eventType}) {message}\n");
        }

        public void WriteRaw(string message)
        {
            File.AppendAllText(path, message);
        }

        public enum EventType : byte
        {
            Info,
            Warning,
            HighWarning,
            Error,
            Fatal,
            Performance
        }
    }
}
