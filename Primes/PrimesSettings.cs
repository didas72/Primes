using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Primes.Common;
using Primes.Common.Files.Settings;

namespace Primes.Exec
{
    public static class PrimesSettings
    {
        public static ushort Threads { get; set; } = 0;
        public static uint PrimeBufferSize { get; set; } = 500;
        public static uint MaxJobQueue { get; set; } = 200;
        public static int FrameTimeMillis { get; set; } = 2000;
        public static bool UseUI { get; set; } = true;



        private static SettingsDocument doc;
        private static readonly Dictionary<string, string> DEFAULTS = new Dictionary<string, string>() { { "threads", "0" }, { "primeBufferSize", "500" }, { "maxJobQueue", "200" }, { "frameTimeMillis", "2000" }, { "useUI", "true" } };//{ "", "" },
        private static string path = null;



        public static void InitSettings(string path)
        {
            PrimesSettings.path = path;
            LoadSettings(path);
        }
        public static void LoadSettings() => LoadSettings(path);
        public static void LoadSettings(string path)
        {
            if (!File.Exists(path))
            {
                LogExtension.LogEvent(Log.EventType.Warning, "No settings file found. Restoring defaults.", "PrimesSettings");

                doc = new SettingsDocument();
                doc.ApplySettingsScheme(DEFAULTS, true);
            }
            else
            {
                LogExtension.LogEvent("Loading settings from file.", "PrimesSettings");

                doc = new SettingsDocument(File.ReadAllText(path));
                doc.ApplySettingsScheme(DEFAULTS, false);

                LogExtension.LogEvent("Parsing settings.", "PrimesSettings");

                ParseSettings();
            }
        }
        public static void SaveSettings() => SaveSettings(path);
        public static void SaveSettings(string path)
        {
            string encoded = doc.ToString();

            File.WriteAllText(path, encoded);
        }
        private static void ParseSettings()
        {
            if (!ushort.TryParse(doc.GetValue("threads"), out ushort threads))
                LogExtension.LogEvent(Log.EventType.Warning, "Failed to parse 'Threads' setting from file, restoring default.", "PrimesSettings");
            else
                Threads = threads;

            if (!uint.TryParse(doc.GetValue("primeBufferSize"), out uint primeBufferSize))
                LogExtension.LogEvent(Log.EventType.Warning, "Failed to parse 'PrimeBufferSize' setting from file, restoring default.", "PrimesSettings");
            else
                PrimeBufferSize = primeBufferSize;

            if (!uint.TryParse(doc.GetValue("maxJobQueue"), out uint maxJobQueue))
                LogExtension.LogEvent(Log.EventType.Warning, "Failed to parse 'MaxJobQueue' setting from file, restoring default.", "PrimesSettings");
            else
                MaxJobQueue = Mathf.Clamp(maxJobQueue, 5, uint.MaxValue);

            if (!int.TryParse(doc.GetValue("frameTimeMillis"), out int frameTimeMillis))
                LogExtension.LogEvent(Log.EventType.Warning, "Failed to parse 'FrameTimeMillis' setting from file, restoring default.", "PrimesSettings");
            else
                FrameTimeMillis = Mathf.Clamp(frameTimeMillis, 200, 60000);

            if (doc.GetValue("useUI").ToLowerInvariant() == "true")
                UseUI = true;
            else if (doc.GetValue("useUI").ToLowerInvariant() == "false")
                UseUI = false;
            else
                LogExtension.LogEvent(Log.EventType.Warning, "Failed to parse 'UseUI' setting from file, restoring default.", "PrimesSettings");
        }
    }
}
