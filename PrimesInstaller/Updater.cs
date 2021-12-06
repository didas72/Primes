using System;
using System.IO;
using System.Threading;
using System.Xml;
using System.Diagnostics;

using DidasUtils.Extensions;
using DidasUtils.Logging;
using DidasUtils.Files;
using DidasUtils.Net;
using DidasUtils;

namespace Primes.Installer
{
    public static class Updater
    {
        private const string versionsURL = "https://raw.githubusercontent.com/didas72/Primes/master/Resources/versions.xml";
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

            string cmdCode = CmdHelper.BuildSelfUpdateCMD(decompressPath, installPath, "PrimesUpdater.exe");
            Log.LogEvent(Log.EventType.DevInfo, $"Update CMD code: {cmdCode}", "SelfUpdate");



            Log.LogEvent("Restarting to update in 3 seconds...", "SelfUpdate");
            Thread.Sleep(3000);
            CmdHelper.LaunchSelfUpdateCMD(cmdCode, tmpDir);



            return UpdateResult.Updated;
        }
        public static UpdateResult UpdatePrimes(string tmpDir, string primesPath)
        {
            Version primesVersion = GetPrimesVersion(ref primesPath);

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

            string cmdCode = CmdHelper.BuildUpdateCMD(decompressPath, installPath, $"Primes {latestVersion} NET Framework 4.7.2");
            Log.LogEvent(Log.EventType.DevInfo, $"Update CMD code: {cmdCode}", "PrimesUpdate");



            Log.LogEvent("Updating...", "PrimesUpdate");
            CmdHelper.RunUpdateCMD(cmdCode, tmpDir);
            Log.LogEvent("Update complete.", "PrimesUpdate");



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



            XmlDocument doc = new();
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



        public static string GetProductXMLCode(Product product)
        {
            return product switch
            {
                Product.Primes_Updater => "primes.updater",
                Product.Primes_Exec => "primes.exec",
                _ => "",
            };
        }
        public static string GetProductURLCode(Product product)
        {
            return product switch
            {
                Product.Primes_Updater => "PrimesUpdater.",
                Product.Primes_Exec => "Primes.",
                _ => "",
            };
        }



        private static Version GetPrimesVersion(ref string primesPath)
        {
            if (string.IsNullOrEmpty(primesPath))
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
            public static readonly Version empty = new(0, 0, 0);



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
                return product switch
                {
                    Product.Primes_Updater => $"u{major}.{minor}.{patch}",
                    _ => $"v{major}.{minor}.{patch}",
                };
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
