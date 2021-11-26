using System;

using DidasUtils.Files;

using BatchServer.Modules;

namespace BatchServer
{
    public static class Globals
    {
        public static string dataPath;
        public static string logPath;
        public static string settingsPath;
        public static string batchesPath;

        public static DbWrapper Db;

        public static SettingsDocument settings;
        public static int maxPerClient;

        public static ClientHandler clHandle;
        public static ControlHandler ctlHandle;
        public static ConnectionListener conListener;
        public static Server server;
    }
}
