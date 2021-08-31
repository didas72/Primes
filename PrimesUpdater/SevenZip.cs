﻿using System;
using System.Diagnostics;

namespace Primes.Updater.Files
{
    public static class SevenZip
    {
        public static void Compress7z(string sourceDir, string outDir)
        {
            ProcessStartInfo i = new ProcessStartInfo
            {
                FileName = "7za.exe",
                Arguments = $"a {outDir} {sourceDir}",
                WindowStyle = ProcessWindowStyle.Hidden
            };


            Process p = Process.Start(i);
            p.WaitForExit();

            if (p.ExitCode != 0 && p.ExitCode != 1) //no error or not fatal
                throw new Exception($"Compression failed! Exit code {p.ExitCode}.");
        }

        public static void Decompress7z(string sourceDir, string outDir)
        {
            ProcessStartInfo i = new ProcessStartInfo
            {
                FileName = "7za.exe",
                Arguments = $"x {sourceDir} -o{outDir} -r",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process p = Process.Start(i);
            p.WaitForExit();

            if (p.ExitCode != 0 && p.ExitCode != 1) //no error or not fatal
                throw new Exception($"Compression failed! Exit code {p.ExitCode}.");
        }
    }
}
