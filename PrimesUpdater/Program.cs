using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Threading;

using Primes.Common;
using Primes.Common.Files;
using Primes.Updater.Files;
using Primes.Updater.Net;

namespace Primes.Updater
{
    class Program
    {
        private const string versionsURL = "https://raw.githubusercontent.com/didas72/Primes/master/PrimesUpdater/Versions/versions.xml";
        private const string releaseLinkBase = "https://github.com/didas72/Primes/releases/download/";
        private const string NET472Ending = ".NET.Framework.4.7.2.7z";
        private static string tmpDir, homeDir;

        private static bool updateSelf;
        private static bool updatePrimes;

        private static volatile bool exiting = false;

        private static void Main(string[] args)
        {
            if (!Init())
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to init.", "MainThread");
                Exit();
            }

            if (!ParseArguments(args))
            {
                Log.LogEvent(Log.EventType.Fatal, "Failed to parse arguments.", "MainThread");
                Exit();
            }

            if (updateSelf)
            {
                int updateSelfRet = UpdateSelf();
                Console.WriteLine($"Update self return: {updateSelfRet}. 0 = success, 1 = already up to date, 2 = error checking version, 3 = error downloading, 4 = error extracting.");
            }

            if (updatePrimes)
            {
                int updatePrimesRet = UpdatePrimes();
                Console.WriteLine($"Update primes return: {updatePrimesRet}. 0 = success, 1 = already up to date, 2 = error checking version, 3 = error downloading, 4 = error extracting.");
            }

            Console.WriteLine("//DONE");
            Console.ReadLine();
        }



        private static bool Init()
        {
            InitDirs();

            InitLog();

            return true;
        }
        private static bool InitDirs()
        {
            Console.WriteLine("Setting up directories...");

            homeDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
            Directory.CreateDirectory(homeDir);

            tmpDir = Path.Combine(tmpDir, "tmp");
            Utils.DeleteDirectory(tmpDir);
            Directory.CreateDirectory(tmpDir);

            Console.WriteLine("Directories set up.");

            return true;
        }
        private static bool InitLog()
        {
            Log.InitLog(Path.Combine(homeDir, "updateLog.txt"));
            Log.InitConsole();

            return true;
        }
        private static bool ParseArguments(string[] args)
        {
            updateSelf = true;
            updatePrimes = true;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLowerInvariant() == "-ns")
                    updateSelf = false;
                else if (args[i].ToLowerInvariant() == "-np")
                    updatePrimes = false;
                else
                {
                    Console.WriteLine("Invalid argument: " + args[i]);
                    Environment.Exit(1);
                }    
            }

            return true;
        }



        private static int UpdateSelf()
        {
            Log.LogEvent("Checking for updater updates...", "SelfUpdate");

            string selfPath = Utils.GetExecutablePath();
            FileVersionInfo selfInfo = FileVersionInfo.GetVersionInfo(selfPath);
            Version selfVersion = new Version(
                (byte)selfInfo.FileMajorPart,
                (byte)selfInfo.FileMinorPart,
                (byte)selfInfo.ProductBuildPart);

            Log.LogEvent($"SelfPath: {selfPath}", "SelfUpdate");
            Log.LogEvent($"SelfVersion: {selfVersion}", "SelfUpdate");

            if (!GetLastestReleaseVersion(Product.Primes_Updater, out Version latestVersion)) return 2; //2 = failed to get version

            Log.LogEvent($"LatestVersion: {latestVersion}", "SelfUpdate");

            if (latestVersion <= selfVersion) return 1; //1 = already up to date



            GetLatestReleaseURL(Product.Primes_Updater, latestVersion, out string latestUrl);

            Log.LogEvent($"LatestUrl: {latestUrl}", "SelfUpdate");

            string updatePath = Path.Combine(tmpDir, "tmp_updater.7z");
            string decompressPath = Path.Combine(tmpDir, "tmp_updater");
            string installPath = Path.GetDirectoryName(selfPath);

            if (!Networking.TryDownloadFile(latestUrl, updatePath)) return 3; //3 = failed to download update

            Log.LogEvent("Latest version downloaded.", "SelfUpdate");

            if (!SevenZip.TryDecompress7z(updatePath, decompressPath)) return 4; //4 = failed to decompress

            Log.LogEvent("Version extracted.", "SelfUpdate");

            string cmdCode = BuildSelfUpdateCMD(decompressPath, installPath, "PrimesUpdater.exe");
            Log.LogEvent($"Update CMD code: {cmdCode}", "SelfUpdate");



            Log.LogEvent("Restarting to update in 3 seconds...", "SelfUpdate");
            Thread.Sleep(3000);
            LaunchSelfUpdateCMD(cmdCode);

            

            return 0; //0 = success
        }
        private static int UpdatePrimes()
        {
            Version selfVersion;

            string primesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
            if (!Directory.Exists(primesPath))
            {
                Log.LogEvent("Primes install not found, installing...", "PrimesUpdate");
                Directory.CreateDirectory(primesPath);
                primesPath = Path.Combine(primesPath, "bin");
                Directory.CreateDirectory(primesPath);
                selfVersion = Version.empty;
                goto primes_install;
            }

            primesPath = Path.Combine(primesPath, "bin");
            if (!Directory.Exists(primesPath))
            {
                Log.LogEvent("Primes install not found, installing...", "PrimesUpdate");
                Directory.CreateDirectory(primesPath);
                selfVersion = Version.empty;
                goto primes_install;
            }

            primesPath = Path.Combine(primesPath, "Primes.exe");
            FileVersionInfo selfInfo = FileVersionInfo.GetVersionInfo(primesPath);
            selfVersion = new Version(
                (byte)selfInfo.FileMajorPart,
                (byte)selfInfo.FileMinorPart,
                (byte)selfInfo.ProductBuildPart);

            Log.LogEvent($"PrimesPath: {primesPath}", "PrimesUpdate");
            Log.LogEvent($"PrimesVersion: {selfVersion}", "PrimesUpdate");



            primes_install:

            if (!GetLastestReleaseVersion(Product.Primes_Exec, out Version latestVersion)) return 2; //2 = failed to get version

            Log.LogEvent($"LatestVersion: {latestVersion}", "PrimesUpdate");

            if (latestVersion <= selfVersion) return 1; //1 = already up to date



            GetLatestReleaseURL(Product.Primes_Exec, latestVersion, out string latestUrl);

            Log.LogEvent($"LatestUrl: {latestUrl}", "PrimesUpdate");

            string updatePath = Path.Combine(tmpDir, "tmp_primes.7z");
            string decompressPath = Path.Combine(tmpDir, "tmp_primes");
            string installPath = Path.GetDirectoryName(primesPath);

            if (!Networking.TryDownloadFile(latestUrl, updatePath)) return 3; //3 = failed to download update

            Log.LogEvent("Latest version downloaded.", "PrimesUpdate");

            if (!SevenZip.TryDecompress7z(updatePath, decompressPath)) return 4; //4 = failed to decompress

            Log.LogEvent("Version extracted.", "PrimesUpdate");

            string cmdCode = BuildUpdateCMD(decompressPath, installPath, $"Primes {latestVersion} NET Framework 4.7.2");
            Log.LogEvent($"Update CMD code: {cmdCode}", "PrimesUpdate");



            Log.LogEvent("Updating...", "PrimesUpdate");
            RunUpdateCMD(cmdCode);
            Log.LogEvent("Update complete.", "PrimesUpdate");



            return 0; //0 = success
        }



        private static bool GetLatestReleaseURL(Product product, Version version, out string url)
        {
            url = $"{releaseLinkBase}{version.ToString(product)}/{GetProductURLCode(product)}{version.ToString(product)}{NET472Ending}";

            return true;
        }
        private static bool GetLastestReleaseVersion(Product product, out Version version)
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



        private static string GetProductXMLCode(Product product)
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
        private static string GetProductURLCode(Product product)
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



        private static string BuildSelfUpdateCMD(string uncompressPath, string installPath, string execToStart)
        {
            return  $"ping -n 3 127.0.0.1>nul\n" +
                    $"taskkill -f -im 7za.exe\n" +
                    $"del /f /q /s \"{installPath}\\*.*\"\n" +
                    $"xcopy \"{uncompressPath}\\*.*\" \"{installPath}\"\n" +
                    $"start \"PrimesUpdater.exe\" \"{installPath}\\{execToStart}\" -ns\n" +
                    $"ping -n 5 127.0.0.1>nul\n" +
                    $"exit";
        }
        private static string BuildUpdateCMD(string uncompressPath, string installPath, string containingDirName)
        {
            return $"ping -n 3 127.0.0.1>nul\n" +
                    $"taskkill -f -im 7za.exe\n" +
                    $"del /f /q /s \"{installPath}\\*.*\"\n" +
                    $"xcopy \"{uncompressPath}\\{containingDirName}\\bin\\*.*\" \"{installPath}\"\n" +
                    $"ping -n 5 127.0.0.1>nul\n" +
                    $"exit";
        }
        private static void LaunchSelfUpdateCMD(string cmdCode)
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
        private static void RunUpdateCMD(string cmdCode)
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



        private static void Exit()
        {
            if (exiting)
            {
                Thread.Sleep(4000);
                return;
            }

            exiting = true;

            Log.Print("Something went wrong!");

            Thread.Sleep(3000);

            Environment.Exit(1);
        }



        private enum Product
        {
            Primes_Updater,
            Primes_Exec,
        }
        private struct Version
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
