using System;
using System.IO;
using System.Diagnostics;

namespace Primes.Updater
{
    public static class CmdHelper
    {
        public static string BuildSelfUpdateCMD(string uncompressPath, string installPath, string execToStart)
        {
            return $"ping -n 3 127.0.0.1>nul\n" +
                    $"taskkill -f -im 7za.exe\n" +
                    $"del /f /q /s \"{installPath}\\*.*\"\n" +
                    $"xcopy \"{uncompressPath}\\*.*\" \"{installPath}\"\n" +
                    $"start \"PrimesUpdater.exe\" \"{installPath}\\{execToStart}\" -ns\n" +
                    $"ping -n 5 127.0.0.1>nul\n" +
                    $"exit";
        }
        public static string BuildUpdateCMD(string uncompressPath, string installPath, string containingDirName)
        {
            return $"@ping -n 3 127.0.0.1>nul\n" +
                    $"@taskkill -f -im 7za.exe\n" +
                    $"for /d %%a in (\"{installPath}\\*\") do rd /S /Q \"%%a\"\n" +
                    $"for %%a in (\"{installPath}\") do if /i not \"%%~nxa\"==\"settings.set\" del /q \"%%a\"\n" +
                    $"xcopy \"{uncompressPath}\\{containingDirName}\\bin\\*.*\" \"{installPath}\"\n" +
                    $"@ping -n 3 127.0.0.1>nul\n" +
                    $"exit";
        }
        public static void LaunchSelfUpdateCMD(string cmdCode, string tmpDir)
        {
            string programPath = Path.Combine(tmpDir, "update.bat");
            File.WriteAllText(programPath, cmdCode);

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = programPath
            };

            Process updater = new Process
            {
                StartInfo = startInfo
            };
            updater.Start();

            Environment.Exit(0);
        }
        public static void RunUpdateCMD(string cmdCode, string tmpDir)
        {
            string programPath = Path.Combine(tmpDir, "update.bat");
            File.WriteAllText(programPath, cmdCode);

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = programPath
            };

            Process updater = new Process
            {
                StartInfo = startInfo
            };
            updater.Start();

            updater.WaitForExit();
        }
    }
}
