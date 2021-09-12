using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Xml;

using Primes.Common;
using Primes.Common.Files;
using Primes.Common.Net;

namespace Primes.Updater
{
    public static class Updater
    {
        private const string versionsURL = "https://raw.githubusercontent.com/didas72/Primes/master/PrimesUpdater/Versions/versions.xml";
        private const string releaseLinkBase = "https://github.com/didas72/Primes/releases/download/";
        private const string NET472Ending = ".NET.Framework.4.7.2.7z";



        public static UpdateResult UpdateSelf(string tmpDir)
        {
            Version selfVersion = GetSelfVersion(out string selfPath);   

            Log.LogEvent($"SelfPath: {selfPath}", "SelfUpdate");
            Log.LogEvent($"SelfVersion: {selfVersion}", "SelfUpdate");

            if (!GetLastestReleaseVersion(Product.Primes_Updater, out Version latestVersion, tmpDir)) return UpdateResult.Failed_Version_Check;

            Log.LogEvent($"LatestVersion: {latestVersion}", "SelfUpdate");

            if (latestVersion <= selfVersion) return UpdateResult.Already_Up_To_Date;



            GetLatestReleaseURL(Product.Primes_Updater, latestVersion, out string latestUrl);

            Log.LogEvent($"LatestUrl: {latestUrl}", "SelfUpdate");

            string updatePath = Path.Combine(tmpDir, "tmp_updater.7z");
            string decompressPath = Path.Combine(tmpDir, "tmp_updater");
            string installPath = Path.GetDirectoryName(selfPath);

            if (!Networking.TryDownloadFile(latestUrl, updatePath)) return UpdateResult.Failed_Download;

            Log.LogEvent("Latest version downloaded.", "SelfUpdate");

            if (!SevenZip.TryDecompress7z(updatePath, decompressPath)) return UpdateResult.Failed_Extraction;

            Log.LogEvent("Version extracted.", "SelfUpdate");

            string cmdCode = BuildSelfUpdateCMD(decompressPath, installPath, "PrimesUpdater.exe");
            Log.LogEvent(Log.EventType.DevInfo, $"Update CMD code: {cmdCode}", "SelfUpdate");



            Log.LogEvent("Restarting to update in 3 seconds...", "SelfUpdate");
            Thread.Sleep(3000);
            LaunchSelfUpdateCMD(cmdCode, tmpDir);



            return UpdateResult.Updated;
        }
        public static UpdateResult UpdatePrimes(string tmpDir)
        {
            Version primesVersion = GetPrimesVersion(out string primesPath);

            Log.LogEvent($"PrimesPath: {primesPath}", "PrimesUpdate");
            Log.LogEvent($"PrimesVersion: {primesVersion}", "PrimesUpdate");



            if (!GetLastestReleaseVersion(Product.Primes_Exec, out Version latestVersion, tmpDir)) return UpdateResult.Failed_Version_Check;
            Log.LogEvent($"LatestVersion: {latestVersion}", "PrimesUpdate");



            if (latestVersion <= primesVersion) return UpdateResult.Already_Up_To_Date;



            GetLatestReleaseURL(Product.Primes_Exec, latestVersion, out string latestUrl);
            Log.LogEvent($"LatestUrl: {latestUrl}", "PrimesUpdate");



            string updatePath = Path.Combine(tmpDir, "tmp_primes.7z");
            string decompressPath = Path.Combine(tmpDir, "tmp_primes");
            string installPath = Path.GetDirectoryName(primesPath);



            if (!Networking.TryDownloadFile(latestUrl, updatePath)) return UpdateResult.Failed_Download;
            Log.LogEvent("Latest version downloaded.", "PrimesUpdate");

            if (!SevenZip.TryDecompress7z(updatePath, decompressPath)) return UpdateResult.Failed_Extraction;
            Log.LogEvent("Version extracted.", "PrimesUpdate");

            string cmdCode = BuildUpdateCMD(decompressPath, installPath, $"Primes {latestVersion} NET Framework 4.7.2");
            Log.LogEvent(Log.EventType.DevInfo, $"Update CMD code: {cmdCode}", "PrimesUpdate");



            string[] settings = null;
            if (File.Exists(Path.Combine(installPath, "Primes.exe.config")))
            {
                settings = File.ReadAllLines(Path.Combine(installPath, "Primes.exe.config"));
            }



            Log.LogEvent("Updating...", "PrimesUpdate");
            RunUpdateCMD(cmdCode, tmpDir);
            Log.LogEvent("Update complete.", "PrimesUpdate");



            if (settings != null)
            {
                string[] newSettings = File.ReadAllLines(Path.Combine(installPath, "Primes.exe.config"));

                if (newSettings.Length == settings.Length)
                {
                    File.WriteAllLines(Path.Combine(installPath, "Primes.exe.config"), settings);
                    Log.LogEvent("Reloading old settings...", "PrimesUpdte");
                }
                else
                {
                    Log.LogEvent(Log.EventType.Warning, "New version includes new settings, which means the updater is unable to preserve current settings.", "PrimesUpdte");
                }
            }



            return UpdateResult.Updated;
        }



        public static bool GetLatestReleaseURL(Product product, Version version, out string url)
        {
            url = $"{releaseLinkBase}{version.ToString(product)}/{GetProductURLCode(product)}{version.ToString(product)}{NET472Ending}";

            return true;
        }
        public static bool GetLastestReleaseVersion(Product product, out Version version, string tmpDir)
        {
            version = Version.empty;

            string versionsPath = Path.Combine(tmpDir, "versions.xml");

            if (!Networking.TryDownloadFile(versionsURL, versionsPath))
                throw new Exception("Failed to download versions file.");



            XmlDocument doc = new XmlDocument();
            doc.Load(versionsPath);
            var versions = doc.LastChild;



            if (!versions.GetFirstChildOfName("stable", out XmlNode stable))
                return false;

            if (!stable.GetFirstChildOfName(GetProductXMLCode(product), out XmlNode primesExec))
                return false;



            byte major = byte.Parse(primesExec.ChildNodes[0].InnerXml);
            byte minor = byte.Parse(primesExec.ChildNodes[1].InnerXml);
            byte patch = byte.Parse(primesExec.ChildNodes[2].InnerXml);

            version = new Version(major, minor, patch);



            return true;
        }



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
                    $"del /f /q /s \"{installPath}\\*.*\"\n" +
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



        public static string GetProductXMLCode(Product product)
        {
            switch (product)
            {
                case Product.Primes_Updater:
                    return "primes.updater";

                case Product.Primes_Exec:
                    return "primes.exec";

                default:
                    return "";
            }
        }
        public static string GetProductURLCode(Product product)
        {
            switch (product)
            {
                case Product.Primes_Updater:
                    return "PrimesUpdater.";

                case Product.Primes_Exec:
                    return "Primes.";

                default:
                    return "";
            }
        }



        private static Version GetPrimesVersion(out string primesPath)
        {
            primesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
            if (!Directory.Exists(primesPath))
            {
                Log.LogEvent("Primes install not found, installing...", "PrimesUpdate");
                Directory.CreateDirectory(primesPath);
                primesPath = Path.Combine(primesPath, "bin");
                Directory.CreateDirectory(primesPath);
                return Version.empty;
            }

            primesPath = Path.Combine(primesPath, "bin");
            if (!Directory.Exists(primesPath))
            {
                Log.LogEvent("Primes install not found, installing...", "PrimesUpdate");
                Directory.CreateDirectory(primesPath);
                return Version.empty;
            }

            primesPath = Path.Combine(primesPath, "Primes.exe");
            if (!File.Exists(primesPath))
            {
                Log.LogEvent("Primes install not found, installing...", "PrimesUpdate");
                return Version.empty;
            }

            FileVersionInfo primesInfo = FileVersionInfo.GetVersionInfo(primesPath);
            return new Version(
                (byte)primesInfo.FileMajorPart,
                (byte)primesInfo.FileMinorPart,
                (byte)primesInfo.ProductBuildPart);
        }
        private static Version GetSelfVersion(out string selfPath)
        {
            selfPath = Utils.GetExecutablePath();
            FileVersionInfo selfInfo = FileVersionInfo.GetVersionInfo(selfPath);
            return new Version(
                (byte)selfInfo.FileMajorPart,
                (byte)selfInfo.FileMinorPart,
                (byte)selfInfo.ProductBuildPart);
        }



        public enum Product
        {
            Primes_Updater,
            Primes_Exec,
        }
        public struct Version
        {
            public readonly byte major, minor, patch;
            public static readonly Version empty = new Version(0, 0, 0);



            public Version(byte major, byte minor, byte patch)
            {
                this.major = major; this.minor = minor; this.patch = patch;
            }



            public static bool operator >=(Version a, Version b)
            {
                if (a.major == b.major && a.minor == b.minor && a.patch == b.patch) return true;
                if (a.major > b.major) return true;
                if (a.minor > b.minor) return true;
                if (a.patch > b.patch) return true;
                return false;
            }
            public static bool operator <=(Version a, Version b)
            {
                if (a.major == b.major && a.minor == b.minor && a.patch == b.patch) return true;

                if (a.major < b.major) return true;
                else if (a.major > b.major) return false;

                if (a.minor < b.minor) return true;
                else if (a.minor > b.minor) return false;

                if (a.patch < b.patch) return true;
                else if (a.patch > b.patch) return false;
                return false;
            }



            public override string ToString()
            {
                return $"v{major}.{minor}.{patch}";
            }
            public string ToString(Product product)
            {
                switch (product)
                {
                    case Product.Primes_Updater:
                        return $"u{major}.{minor}.{patch}";

                    case Product.Primes_Exec:
                    default:
                        return $"v{major}.{minor}.{patch}";
                }
            }
        }
        public enum UpdateResult
        {
            Updated,
            Already_Up_To_Date,
            Failed_Version_Check,
            Failed_Download,
            Failed_Extraction,
        }
    }
}
