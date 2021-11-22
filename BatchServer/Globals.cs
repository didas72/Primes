using System;

using DidasUtils.Files;

namespace BatchServer
{
    public static class Globals
    {
        public static string dataPath;
        public static string logPath;
        public static string settingsPath;

        public static DbWrapper Db;

        public static SettingsDocument settings;
    }
}
