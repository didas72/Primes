using System;
using System.Diagnostics;

namespace Primes.Common.Files
{
    /// <summary>
    /// Class containing methods to use the SevenZip utility.
    /// </summary>
    public static class SevenZip
    {
        /// <summary>
        /// Compresses a directory or file into a .7z file.
        /// </summary>
        /// <param name="sourceDir">The directory or file to compress.</param>
        /// <param name="outDir">The output file.</param>
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
        /// <summary>
        /// Decompresses a.7z file.
        /// </summary>
        /// <param name="sourceDir">The file to decompress</param>
        /// <param name="outDir">The directory to store the decompressed files to.</param>
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
        /// <summary>
        /// Compresses a directory or file into a .7z file.
        /// </summary>
        /// <param name="sourceDir">The directory or file to compress.</param>
        /// <param name="outDir">The output file.</param>
        /// <returns>Boolean indicating the operation's success.</returns>
        public static bool TryCompress7z(string sourceDir, string outDir)
        {
            try
            {
                Compress7z(sourceDir, outDir);
            }
            catch { return false; }
            return true;
        }
        /// <summary>
        /// Decompresses a.7z file.
        /// </summary>
        /// <param name="sourceDir">The file to decompress</param>
        /// <param name="outDir">The directory to store the decompressed files to.</param>
        /// <returns>Boolean indicating the operation's success.</returns>
        public static bool TryDecompress7z(string sourceDir, string outDir)
        {
            try
            {
                Decompress7z(sourceDir, outDir);
            }
            catch { return false; }
            return true;
        }
    }
}
