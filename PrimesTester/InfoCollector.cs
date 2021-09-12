using System;
using System.IO;
using System.Management;

namespace Primes.Tester
{
    public class InfoCollector
    {
        public string GetDetails()
        {
            string outp = "CPU Details\n";
            outp += "==============================\n";

            foreach (var info in new ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                outp += $"Name: {info["Name"]}\n";
                outp += $"Manufacturer: {info["Manufacturer"]}\n";
                outp += $"Cores: {info["NumberOfCores"]}\n";
                outp += $"Threads: {info["NumberOfLogicalProcessors"]}\n";
                outp += $"Data width: {info["DataWidth"]} bits\n";
                outp += $"Address width: {info["AddressWidth"]} bits\n";
                outp += $"Max clock speed: {info["MaxClockSpeed"]} MHz\n";
                outp += $"Current closk speed: {info["CurrentClockSpeed"]} MHz\n";
                outp += $"Current voltage: {info["CurrentVoltage"]} * 0.1 V\n";
                outp += $"L2 cache size: {info["L2CacheSize"]} kB\n";
                outp += $"L2 cache speed: {info["L2CacheSpeed"]} MHz\n";
                outp += $"L3 cache size: {info["L3CacheSize"]} kB\n";
                outp += $"L3 cache speed: {info["L3CacheSpeed"]} MHz\n";
                outp += $"Description: {info["Description"]}\n";
                outp += "==============================\n";
            }

            outp += "OS Details\n";
            outp += "==============================\n";
            outp += $"Platform: {Environment.OSVersion.Platform}\n";
            outp += $"Version: {Environment.OSVersion.VersionString}\n";
            outp += $"64 bit: {Environment.Is64BitOperatingSystem}\n";
            outp += "==============================\n";

            outp += "Drive Details\n";
            outp += "==============================\n";

            foreach(DriveInfo d in DriveInfo.GetDrives())
            {
                try
                {
                    string tmp = string.Empty;

                    tmp += $"Drive name: {d.Name}\n";
                    tmp += $"Type: {d.DriveType}\n";
                    tmp += $"Format: {d.DriveFormat}\n";
                    tmp += $"Size: {d.TotalSize} B\n";
                    tmp += $"Free space: {d.TotalFreeSpace} B\n";
                    tmp += "==============================\n";

                    outp += tmp;
                }
                catch { continue; }
            }

            return outp;
        }
    }
}
