using System;
using System.Collections.Generic;
using System.Timers;

using DidasUtils.Files;

namespace BatchServer
{
    internal static class Globals
    {
        public static string startLogPath;

        public static string homeDir;
        public static string sourceDir;
        public static string completeDir;
        public static string cacheDir;

        public static Timer ExpireElementsTimer;
        public static ClientData clientData;
        public static ClientListener clientListener;
        public static SettingsDocument settings;
    }
}
