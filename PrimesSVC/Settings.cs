using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using DidasUtils.Logging;

using Microsoft.Win32;

namespace Primes.SVC
{
    internal static class Settings
    {
        //internals
        private static RegistryKey homeKey;




        //properties
        public static string HomeDir { get => GetHomeDir(); set => SetHomeDir(value); }
        public static int Threads { get => GetThreads(); set => SetThreads(value); }
        public static ushort ControlPort { get => GetControlPort(); set => SetControlPort(value); }
        public static bool AllowExternalControl { get => GetAllowExternalControl(); set => SetAllowExternalControl(value); }
        public static int PrimeBufferSize { get => GetPrimeBufferSize(); set => SetPrimeBufferSize(value); }
        public static int MaxJobQueue { get => GetMaxJobQueue(); set => SetMaxJobQueue(value); }
        public static int MaxResourceMemory { get => GetMaxResourceMemory(); set => SetMaxResourceMemory(value); }



        public static bool InitSettings_WinReg()
        {
            try
            {
                RegistryKey softKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", true);
                if (softKey == null)
                {
                    Log.LogEvent(Log.EventType.Error, "Failed to access SOFTWARE key.", "InitSettings_WinReg");
                    return false;
                }

                RegistryKey companyKey = softKey.CreateSubKey(Globals.Company);
                homeKey = companyKey.CreateSubKey(Globals.Product);
            }
            catch (Exception e)
            {
                Log.LogException("Failed to init settings from registry.", "InitSettings_WinReg", e);
                return false;
            }

            return true;
        }
    


        private static bool HasRegValue(string value)
        {
            if (homeKey == null)
                throw new Exception("Attempt to check from uninitialized settings.");

            try
            {
                return homeKey.GetValueNames().Contains(value);
            }
            catch
            {
                return false;
            }
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
            if (homeKey == null)
                throw new Exception("Attempt to write to uninitialized settings.");

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



        //getters and setters (SLOW)
        public static string GetHomeDir()
        {
            if (!HasRegValue("HomeDir"))
                SetHomeDir(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "primes"));

            GetRegValue("HomeDir", out object outp);

            return (string)outp;
        }
        public static void SetHomeDir(string value)
        {
            SetRegValue("HomeDir", value, RegistryValueKind.String);
        }
        
        public static int GetThreads()
        {
            if (!HasRegValue("Threads"))
                SetThreads(4);

            GetRegValue("Threads", out object outp);

            return (int)outp;
        }
        public static void SetThreads(int value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value) , "The number of threads must be a positive number.");

            SetRegValue("Threads", value, RegistryValueKind.DWord);
        }

        public static ushort GetControlPort()
        {
            if (!HasRegValue("ControlPort"))
                SetControlPort(13031);

            GetRegValue("ControlPort", out object outp);

            return (ushort)outp;
        }
        public static void SetControlPort(ushort value)
        {
            if (value < 1024)
                throw new ArgumentOutOfRangeException(nameof(value), "The port number must be greater than 1023 to avoid interference with common applications.");

            SetRegValue("ControlPort", value, RegistryValueKind.Binary);
        }

        public static bool GetAllowExternalControl()
        {
            if (!HasRegValue("AllowExternalControl"))
                SetAllowExternalControl(true);

            GetRegValue("AllowExternalControl", out object outp);

            return (bool)outp;
        }
        public static void SetAllowExternalControl(bool value)
        {
            SetRegValue("AllowExternalControl", value ? 1 : 0, RegistryValueKind.DWord);
        }

        public static int GetPrimeBufferSize()
        {
            if (!HasRegValue("PrimeBufferSize"))
                SetPrimeBufferSize(1024);

            GetRegValue("PrimeBufferSize", out object outp);

            return (int)outp;
        }
        public static void SetPrimeBufferSize(int value)
        {
            SetRegValue("PrimeBufferSize", value, RegistryValueKind.DWord);
        }

        public static int GetMaxJobQueue()
        {
            if (!HasRegValue("MaxJobQueue"))
                SetMaxJobQueue(100);

            GetRegValue("MaxJobQueue", out object outp);

            return (int)outp;
        }
        public static void SetMaxJobQueue(int value)
        {
            SetRegValue("MaxJobQueue", value, RegistryValueKind.DWord);
        }

        public static int GetMaxResourceMemory()
        {
            if (!HasRegValue("MaxResourceMemory"))
                SetMaxResourceMemory(-1);

            GetRegValue("MaxResourceMemory", out object outp);

            return (int)outp;
        }
        public static void SetMaxResourceMemory(int value)
        {
            SetRegValue("MaxResourceMemory", value, RegistryValueKind.DWord);
        }
    }
}
