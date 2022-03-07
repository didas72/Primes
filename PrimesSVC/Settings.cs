using System;
using System.IO;
using System.Runtime.InteropServices;

using DidasUtils.Logging;

using Microsoft.Win32;

namespace Primes.SVC
{
    internal static class Settings
    {
        //internals
        private static RegistryKey homeKey;

        //backing fields
        private static string homeDir;
        private static int threads = -1;

        //properties
        public static string HomeDir { get => GetHomeDir(); set => SetHomeDir(value); }
        public static int Threads { get => GetThreads(); set => SetThreads(value); }



        public static bool InitSettings_WinReg()
        {
            try
            {
                RegistryKey softKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\");
                if (softKey == null)
                {
                    Log.LogEvent(Log.EventType.Error, "Failed to access SOFTWARE key.", "InitSettings_WinReg");
                    return false;
                }

                RegistryKey companyKey = softKey.CreateSubKey(Globals.Company);
                homeKey = companyKey.CreateSubKey(Globals.Product);

                if (!GetRegValue("HomeDir", out object HomeDir))
                {
                    Log.LogEvent(Log.EventType.Warning, "Failed to read HomeDir from registry, setting default value.", "InitSettings_WinReg");
                    SetHomeDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "primes"));
                }

                //TODO: Add coming settings
            }
            catch (Exception e)
            {
                Log.LogException("Failed to init settings from registry.", "InitSettings_WinReg", e);
                return false;
            }

            return true;
        }



        private static bool GetRegValue(string value, out object obj)
        {
            obj = null;

            if (homeKey == null)
                throw new Exception("Attempt to read from uninitialized settings.");

            try
            {
                obj = homeKey.GetValue(value);
            }
            catch (Exception e)
            {
                Log.LogException($"Failed to fetch setting '{value}' from registry.", "GetRegValue", e);
                return false;
            }

            return true;
        }
        private static bool SetRegValue(string value, object obj, RegistryValueKind kind)
        {
            obj = null;

            if (homeKey == null)
                throw new Exception("Attempt to read from uninitialized settings.");

            try
            {
                homeKey.SetValue(value, obj, kind);
            }
            catch (Exception e)
            {
                Log.LogException($"Failed to set setting '{value}' in registry.", "SetRegValue", e);
                return false;
            }

            return true;
        }



        //getters and setters
        public static string GetHomeDir()
        {
            if (string.IsNullOrEmpty(homeDir))
                throw new Exception("Attempt to read from an uninitialized homedir setting.");

            return homeDir;
        }
        public static void SetHomeDir(string value)
        {
            if (!string.IsNullOrEmpty(homeDir)) //not in init
                SetRegValue("HomeDir", value, RegistryValueKind.String);

            homeDir = value;
        }
        
        public static int GetThreads()
        {
            if (threads < 1)
                throw new Exception("Attempt to read from an uninitialized or invalid threads setting.");

            return threads;
        }
        public static void SetThreads(int value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value) , "The number of threads must be a positive number.");

            if (threads != -1)
                SetRegValue("Threads", value, RegistryValueKind.Binary);

            threads = value;
        }
    }
}
