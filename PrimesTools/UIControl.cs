using System;
using System.Linq;
using System.IO;
using System.Windows.Controls;
using System.Diagnostics;

using Microsoft.Win32;
using Microsoft.VisualBasic;

using DidasUtils;
using DidasUtils.Logging;

using Primes.Common;
using Primes.Common.Files;

namespace PrimesTools
{
    public static class UIControl
    {
        public static ListView Header, Primes, Binary, Stats;
        public static TextBlock Status, Usage;
        public static ProgressBar Progress;


        private static PrimeJob job;
        private static byte[] bytes;



        public static void Init(ListView header, ListView primes, ListView binary, ListView stats, TextBlock status, ProgressBar progress, TextBlock usage)
        {
            Header = header; Primes = primes; Binary = binary; Stats = stats; Status = status; Progress = progress; Usage = usage;

            EmptyAllLists();
            SetStatus("Ready.");
            SetProgress(0d);

            Log.InitLog(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }



        public static void OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Choose prime job file",
                DefaultExt = "primejob",
                Filter = "PrimeJob files|*.primejob|All files|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes"),
                Multiselect = false,
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    CloseFile();

                    SetStatus("Loading file...");

                    SetProgress(0d);
                    LoadJob(dialog.FileName);
                    SetStatus("Drawing header...");

                    SetProgress(0.2d);
                    DrawHeader();
                    SetStatus("Drawing primes...");

                    SetProgress(0.4d);
                    DrawPrimes();
                    SetStatus("Drawing binary...");

                    SetProgress(0.6d);
                    DrawBinary();
                    SetStatus("Drawing Stats...");

                    SetProgress(0.8d);
                    DrawStats();
                    SetStatus("Loaded.");

                    SetProgress(0);
                }
                catch
                {
                    SetStatus($"Error opening file.");
                }
            }
            else
            {
                SetStatus("No file chosen.");
            }
        }
        public static void CloseFile()
        {
            EmptyAllLists();
            job = null;
            bytes = null;
            SetStatus("File closed.");
        }
        public static void SaveFile()
        {
            if (job == null)
                SetStatus("No job loaded.");

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Choose save location",
                DefaultExt = "primejob",
                Filter = "PrimeJob files|*.primejob|All files|*.*",
                CheckFileExists = false,
                CheckPathExists = true,
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes"),
                AddExtension = true,
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    PrimeJob.Serialize(job, dialog.FileName);

                    SetStatus("File saved.");
                }
                catch
                {
                    SetStatus("Error saving file.");
                }
            }
            else
            {
                SetStatus("No file chosen.");
            }
        }
        public static void SelectedPrime(int selIndex) => SelectedPrime(selIndex, true);
        public static void SelectedPrime(int selIndex, bool showAddr)
        {
            if (selIndex == -1)
                return;

            if (selIndex >= job.Primes.Count)
                return;

            long address = CalculateBinaryAddress(selIndex);

            if (showAddr)
                SetStatus($"Addr: {address:X8}");

            int index = (int)address / 8;

            Binary.SelectedIndex = index;
            Binary.ScrollIntoView(Binary.Items[index]);

            Primes.SelectedIndex = selIndex;
            Primes.ScrollIntoView(Primes.Items[selIndex]);
        }
        public static void ROCheckJob()
        {
            if (job == null)
                SetStatus("No job loaded.");

            try
            {
                if (!PrimeJob.CheckJob(job, false, out string msg))
                {
                    File.WriteAllText($"checkj_{job.Start}.log.txt", msg);
                    SetStatus("File failed check.");

                    ProcessCheckLog(msg);
                }
                else
                    SetStatus("File passed check.");
            }
            catch
            {
                SetStatus("Error checking file.");
            }
        }
        public static void RDCheckJob()
        {
            if (job == null)
                SetStatus("No job loaded.");

            try
            {
                if (!PrimeJob.CheckJob(job, true, out string msg))
                {
                    File.WriteAllText($"checkj_{job.Start}.log.txt", msg);

                    if (!PrimeJob.CheckJob(job, false, out string msg2))
                    {
                        File.WriteAllText($"checkj_{job.Start}_second_pass.log.txt", msg2);
                        SetStatus("Severe corruption.");

                        ProcessCheckLog(msg);
                    }
                    else
                    {
                        MemoryStream s = new MemoryStream();
                        PrimeJob.Serialize(job, s);
                        LoadJob(s);
                        DrawHeader();
                        DrawPrimes();
                        DrawBinary();
                        DrawStats();

                        s.Dispose();

                        SetStatus("Duplicates corrected. Reloaded.");
                    }
                }
                else
                    SetStatus("File passed check.");
            }
            catch
            {
                SetStatus("Error checking file.");
            }
        }
        public static void ROCheckFolder()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog()
            {
                Description = "Select folder to check...",
                RootFolder = Environment.SpecialFolder.UserProfile,
                ShowNewFolderButton = false,
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string[] files = Utils.GetSubFiles(dialog.SelectedPath, "*.primejob");
                    int good = 0;
                    int bad = 0;

                    foreach (string f in files)
                    {
                        PrimeJob j = PrimeJob.Deserialize(f);

                        if (!PrimeJob.CheckJob(j, false, out string msg))
                        {
                            File.WriteAllText($"checkj_{j.Start}.log.txt", msg);
                            bad++;
                        }
                        else
                            good++;
                    }

                    if (bad != 0)
                        SetStatus($"Folder failed {bad}/{bad + good} checks.");
                    else
                        SetStatus($"Folder passed check ({good}).");
                }
                catch (Exception e)
                {
                    Log.LogException("Error checking folder (RO).", "UIControls", e);
                    SetStatus("Error checking folder.");
                }
            }
            else
            {
                SetStatus("No folder chosen.");
            }
        }
        public static void RDCheckFolder()
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog()
            {
                Description = "Select folder to check...",
                RootFolder = Environment.SpecialFolder.UserProfile,
                ShowNewFolderButton = false,
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string[] files = Utils.GetSubFiles(dialog.SelectedPath, "*.primejob");
                    int good = 0, bad = 0, fix = 0;

                    foreach (string f in files)
                    {
                        PrimeJob j = PrimeJob.Deserialize(f);

                        if (!PrimeJob.CheckJob(j, true, out string msg))
                        {
                            File.WriteAllText($"checkj_{j.Start}.log.txt", msg);

                            if (!PrimeJob.CheckJob(j, false, out string msg2))
                            {
                                File.WriteAllText($"checkj_{j.Start}_second_pass.log.txt", msg2);
                                bad++;
                            }
                            else
                            {
                                PrimeJob.Serialize(j, f);
                                fix++;
                            }
                        }
                        else
                            good++;
                    }

                    if (bad != 0 || fix != 0)
                        SetStatus($"Bad/Fix/Good {bad}/{fix}/{good}.");
                    else
                        SetStatus($"Folder passed check ({good}).");
                }
                catch
                {
                    SetStatus("Error checking folder.");
                }
            }
            else
            {
                SetStatus("No folder chosen.");
            }
        }
        public static void JumpToPrime()
        {
            string resp = Interaction.InputBox("Prime index:", "Jump to prime", "-1");

            if (!int.TryParse(resp, out int index))
            {
                SetStatus("Invalid index.");
                return;
            }

            if (index < 0 || index >= Primes.Items.Count)
            {
                SetStatus("Index out of range.");
                return;
            }

            SelectedPrime(index);
        }
        public static void JumpToBinary()
        {
            string resp = Interaction.InputBox("Binary address (hex):", "Jump to binary", "-1");

            try
            {
                int index = int.Parse(resp, System.Globalization.NumberStyles.HexNumber) / 8;

                if (index < 0 || index >= Primes.Items.Count)
                {
                    SetStatus("Address out of range.");
                    return;
                }

                Binary.SelectedIndex = index;
                Binary.ScrollIntoView(Binary.Items[index]);
            }
            catch
            {
                SetStatus("Invalid address.");
                return;
            }
        }
        public static void FindPrime()
        {
            string resp = Interaction.InputBox("Prime index:", "Jump to prime", "-1");

            if (!ulong.TryParse(resp, out ulong prime))
            {
                SetStatus("Invalid index.");
                return;
            }

            int bot = 0, top = job.Primes.Count, i;

            while (true)
            {
                i = (bot + top) / 2;

                if (prime < job.Primes[i])
                    top = i;
                else if (prime > job.Primes[i])
                    bot = i;
                else if (prime == job.Primes[i])
                    break;

                if (top - bot <= 1)
                    break;
            }

            SelectedPrime(i);
        }
        public static void FindNCCBigJump()
        {
            if (job.FileVersion.Equals(new PrimeJob.Version(1, 2, 0)) && job.FileCompression.NCC)
            {
                for (int i = 40; i + 1 < bytes.Length; i += 2)
                {
                    if (bytes[i] == 0 && bytes[i + 1] == 0)
                    {
                        int index = i / 8;

                        Binary.SelectedIndex = index;
                        Binary.ScrollIntoView(Binary.Items[index]);

                        SetStatus($"First address {i:X8}");
                        return;
                    }
                }

                SetStatus("No big jumps found.");
            }

            SetStatus("File does not use/support NCC.");
        }
        public static void IsPrime()
        {
            string resp = Interaction.InputBox("Number to check:", "Is prime", "0");

            if (!ulong.TryParse(resp, out ulong value))
            {
                SetStatus("Invalid value.");
                return;
            }

            if (PrimesMath.IsPrime(value, out ulong divider))
                SetStatus($"{value} is prime.");
            else
                SetStatus($"{value} is dividable by {divider}.");

        }
        public static void Update()
        {
            UpdateMemoryUsage();
        }



        private static void UpdateMemoryUsage()
        {
            long used = Process.GetCurrentProcess().PrivateMemorySize64;

            SetMemoryUsage($"Memory usage: {Utils.FormatSize(used)}");
        }



        private static void LoadJob(string path)
        {
            FileStream fs = File.OpenRead(path);

            LoadJob(fs);
        }
        private static void LoadJob(Stream s)
        {
            if (job == null)
                job = new PrimeJob(PrimeJob.Version.Zero, 0, 0);

            lock (job)
            {
                s.Seek(0, SeekOrigin.Begin);
                bytes = new byte[s.Length];
                s.Read(bytes, 0, bytes.Length);
                job = PrimeJob.Deserialize(s);
            }
        }
        private static void DrawHeader()
        {
            lock (Header)
            {
                Header.Items.Add(new ValueItemPair("Version", job.FileVersion.ToString()));
                Header.Items.Add(new ValueItemPair("Compression", Convert.ToString(job.FileCompression.GetByte(), 2).PadLeft(8, '0')));
                Header.Items.Add(new ValueItemPair("Batch", job.Batch.ToString()));
                Header.Items.Add(new ValueItemPair("Start", job.Start.ToString()));
                Header.Items.Add(new ValueItemPair("Count", job.Count.ToString()));
                Header.Items.Add(new ValueItemPair("Progress", job.Progress.ToString()));
                Header.Items.Add(new ValueItemPair("Status (Computed)", job.PeekStatus().ToString()));
            }
        }
        private static void DrawPrimes()
        {
            lock (Primes)
            {
                for (int i = 0; i < job.Primes.Count; i++)
                    Primes.Items.Add(new ValueItemPair(i.ToString(), job.Primes[i].ToString()));
            }
        }
        private static void DrawBinary()
        {
            lock (Binary)
            {
                int lines = Mathf.DivideRoundUp(bytes.Length, 8);
                int last = bytes.Length % 8;

                int i;

                for (i = 0; i < lines - 1; i += 1)
                {
                    Binary.Items.Add(new AddressValuesPair((i * 8).ToString("X6"), bytes[i * 8], bytes[i * 8 + 1], bytes[i * 8 + 2], bytes[i * 8 + 3], bytes[i * 8 + 4], bytes[i * 8 + 5], bytes[i * 8 + 6], bytes[i * 8 + 7]));
                }

                i *= 8;

                switch (last)
                {
                    case 0:
                        Binary.Items.Add(new AddressValuesPair((i * 8).ToString("X6"), bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3], bytes[i + 4], bytes[i + 5], bytes[i + 6], bytes[i + 7]));
                        break;

                    case 1:
                        Binary.Items.Add(new AddressValuesPair((i * 8).ToString("X6"), bytes[i], 0, 0, 0, 0, 0, 0, 0));
                        break;

                    case 2:
                        Binary.Items.Add(new AddressValuesPair((i * 8).ToString("X6"), bytes[i], bytes[i + 1], 0, 0, 0, 0, 0, 0));
                        break;

                    case 3:
                        Binary.Items.Add(new AddressValuesPair((i * 8).ToString("X6"), bytes[i], bytes[i + 1], bytes[i + 2], 0, 0, 0, 0, 0));
                        break;

                    case 4:
                        Binary.Items.Add(new AddressValuesPair((i * 8).ToString("X6"), bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3], 0, 0, 0, 0));
                        break;

                    case 5:
                        Binary.Items.Add(new AddressValuesPair((i * 8).ToString("X6"), bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3], bytes[i + 4], 0, 0, 0));
                        break;

                    case 6:
                        Binary.Items.Add(new AddressValuesPair((i * 8).ToString("X6"), bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3], bytes[i + 4], bytes[i + 5], 0, 0));
                        break;

                    case 7:
                        Binary.Items.Add(new AddressValuesPair((i * 8).ToString("X6"), bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3], bytes[i + 4], bytes[i + 5], bytes[i + 6], 0));
                        break;
                }
            }
        }
        private static void DrawStats()
        {
            lock (Stats)
            {
                long rawSizeB = PrimeJob.RawFileSize(job);
                long NCCSizeB = bytes.Length;
                float NCCRatio = NCCSizeB / (float)rawSizeB;
                int NCCBigJumps = 0;

                for (int i = 40; i + 1 < bytes.Length; i += 2)
                {
                    if (bytes[i] == 0 && bytes[i + 1] == 0)
                    {
                        NCCBigJumps++;
                    }
                }

                Stats.Items.Add(new ValueItemPair("Total primes:", job.Primes.Count.ToString()));
                Stats.Items.Add(new ValueItemPair("Size (raw):", Utils.FormatSize(rawSizeB)));
                Stats.Items.Add(new ValueItemPair("Size (NCC):", Utils.FormatSize(NCCSizeB)));
                Stats.Items.Add(new ValueItemPair("NCC ratio:", NCCRatio.ToString("0.00%")));
                Stats.Items.Add(new ValueItemPair("NCC Big Jumps:", NCCBigJumps.ToString()));
            }
        }



        private static void ProcessCheckLog(string msg)
        {
            string line = msg.Substring(0, msg.IndexOf('\n'));

            if (line.StartsWith("Prime at index "))//15
            {
                string indexS = msg.Substring(15);
                indexS = indexS.Substring(0, msg.IndexOf(' ') + 1);

                if (int.TryParse(indexS, out int index))
                {
                    SelectedPrime(index, false);
                    SetStatus($"Log: {line}");
                }
            }
        }
        private static long CalculateBinaryAddress(int index)
        {
            long address = PrimeJob.HeaderSize(job);
            address += Compression.NCC.Compress(new ArraySegment<ulong>(job.Primes.ToArray(), 0, index).ToArray()).LongLength;

            return address;
        }



        private static void EmptyAllLists()
        {
            Header.Items.Clear();
            Binary.Items.Clear();
            Primes.Items.Clear();
            Stats.Items.Clear();
        }
        private static void SetStatus(string status) => Status.Text = status;
        private static void SetProgress(double progress) => Progress.Value = progress * 100d;
        private static void SetMemoryUsage(string usage) => Usage.Text = usage;
    }
}
