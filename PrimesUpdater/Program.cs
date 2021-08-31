using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using Primes;
using Primes.Common;
using Primes.Common.Files;
using Primes.Updater.Files;
using Primes.Updater.Net;

namespace Primes.Updater
{
    class Program
    {
        const string versionsURL = "https://raw.githubusercontent.com/didas72/Primes/master/PrimesUpdater/Versions/versions.xml";
        const string releaseLinkBase = "https://github.com/didas72/Primes/releases/download/";
        const string NET472Ending = ".NET.Framework.4.7.2.7z";
        static string tmpDir;

        static void Main(string[] args)
        {
            InitDirs();

            int updateSelfRet = UpdateSelf();
            Console.WriteLine($"Update self return: {updateSelfRet}");

            Console.WriteLine("//DONE");
            Console.ReadLine();
        }



        public static bool InitDirs()
        {
            Console.WriteLine("Setting up directories...");

            tmpDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
            Directory.CreateDirectory(tmpDir);

            tmpDir = Path.Combine(tmpDir, "tmp");
            Directory.CreateDirectory(tmpDir);

            Console.WriteLine("Directories set up.");

            return true;
        }



        public static int UpdateSelf()
        {
            string selfPath = Utils.GetExecutablePath();
            FileVersionInfo selfInfo = FileVersionInfo.GetVersionInfo(selfPath);
            Version selfVersion = new Version(
                (byte)selfInfo.FileMajorPart,
                (byte)selfInfo.FileMinorPart,
                (byte)selfInfo.ProductBuildPart);

            Console.WriteLine($"SelfPath: {selfPath}");
            Console.WriteLine($"SelfVersion: {selfVersion}");

            if (!GetLastestReleaseVersion(Product.Primes_Updater, out Version latestVersion)) return 2; //2 = failed to get version


            Console.WriteLine($"LatestVersion: {latestVersion}");

            if (latestVersion <= selfVersion) return 1; //1 = already up to date



            GetLatestReleaseURL(Product.Primes_Updater, latestVersion, out string latestUrl);

            Console.WriteLine($"LatestUrl: {latestUrl}");

            string updatePath = Path.Combine(tmpDir, "tmp_updater.7z");
            string decompressPath = Path.Combine(tmpDir, "tmp_updater");
            string installPath = Path.GetDirectoryName(selfPath);

            if (!Networking.TryDownloadFile(latestUrl, updatePath)) return 3; //3 = failed to download update

            if (!SevenZip.TryDecompress7z(updatePath, decompressPath)) return 4; //4 = failed to decompress

            string cmdCode = BuildUpdateCMD(decompressPath, installPath, "PrimesUpdater.exe");
            Console.WriteLine($"Update CMD code: {cmdCode}");



            LaunchSelfUpdateCMD(cmdCode);

            

            return 0; //success
        }



        public static bool GetLatestReleaseURL(Product product, Version version, out string url)
        {
            url = $"{releaseLinkBase}{version}/{GetProductURLCode(product)}{version}{NET472Ending}";

            return true;
        }
        public static bool GetLastestReleaseVersion(Product product, out Version version)
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



        public static string BuildUpdateCMD(string uncompressPath, string installPath, string execToStart)
        {
            return $@"ping -n 3 127.0.0.1>nul
rm -r '{installPath}'
xcopy '{uncompressPath}\*' '{Path.GetDirectoryName(installPath)}'
start '{installPath}\{execToStart}'
ping -n 3 127.0.0.1>nul
exit";
        }
        private static void LaunchSelfUpdateCMD(string cmdCode)
        {
            string programPath = Path.Combine(tmpDir, "update.bat");
            File.WriteAllText(programPath, cmdCode);

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "update.bat"
            };

            Process updater = new Process
            {
                StartInfo = startInfo
            };
            updater.Start();

            Environment.Exit(0);
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



            public static bool operator >(Version a, Version b)
            {
                if (a.major > b.major) return true;
                if (a.minor > b.minor) return true;
                if (a.patch > b.minor) return true;
                return false;
            }
            public static bool operator <(Version a, Version b)
            {
                if (a.major > b.major) return false;
                if (a.minor > b.minor) return false;
                if (a.patch > b.minor) return false;
                return true;
            }
            public static bool operator >=(Version a, Version b)
            {
                if (a.major >= b.major) return true;
                if (a.minor >= b.minor) return true;
                if (a.patch >= b.minor) return true;
                return false;
            }
            public static bool operator <=(Version a, Version b)
            {
                if (a.major > b.major) return false;
                if (a.minor > b.minor) return false;
                if (a.patch > b.minor) return false;
                return true;
            }



            public override string ToString()
            {
                return $"v{major}.{minor}.{patch}";
            }
            public string ToString(Product product)
            {
                switch(product)
                {
                    case Product.Primes_Updater:
                        return $"u{major}.{minor}.{patch}";

                    case Product.Primes_Exec:
                    default:
                        return $"v{major}.{minor}.{patch}";
                }
            }
        }
    }
}
