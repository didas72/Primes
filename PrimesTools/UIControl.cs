using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Controls;
using System.Diagnostics;

using Microsoft.Win32;
using Microsoft.VisualBasic;

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
        public static void SelectedPrime(int selIndex)
        {
            if (selIndex == -1)
                return;

            if (selIndex >= job.Primes.Count)
                return;

            long address = PrimeJob.HeaderSize(job);
            address += Compression.NCC.Compress(new ArraySegment<ulong>(job.Primes.ToArray(), 0, selIndex).ToArray()).LongLength;

            SetStatus($"Addr: {address:X8}");

            int index = (int)address / 8;

            Binary.SelectedIndex = index;
            Binary.ScrollIntoView(Binary.Items[index]);
        }
        public static void ROCheck()
        {
            if (job == null)
                SetStatus("No job loaded.");

            try
            {
                if (!PrimeJob.CheckJob(job, false, out string msg))
                {
                    File.WriteAllText($"check_{job.Start}.log.txt", msg);
                    SetStatus("File failed check.");
                }
                else
                    SetStatus("File passed check.");
            }
            catch
            {
                SetStatus("Error checking file.");
            }
        }
        public static void RDCheck()
        {
            if (job == null)
                SetStatus("No job loaded.");

            try
            {
                if (!PrimeJob.CheckJob(job, true, out string msg))
                {
                    File.WriteAllText($"check_{job.Start}.log.txt", msg);

                    if (!PrimeJob.CheckJob(job, false, out string msg2))
                    {
                        File.WriteAllText($"check_{job.Start}_second_pass.log.txt", msg2);
                        SetStatus("Severe corruption.");
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

            Primes.SelectedIndex = index;
            Primes.ScrollIntoView(Primes.Items[index]);

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

            Primes.SelectedIndex = i;
            Primes.ScrollIntoView(Primes.Items[i]);

            SelectedPrime(i);
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

                Stats.Items.Add(new ValueItemPair("Size (raw):", Utils.FormatSize(rawSizeB)));
                Stats.Items.Add(new ValueItemPair("Size (NCC):", Utils.FormatSize(NCCSizeB)));
                Stats.Items.Add(new ValueItemPair("NCC ratio:", NCCRatio.ToString("0.00%")));
                Stats.Items.Add(new ValueItemPair("Total primes:", job.Primes.Count.ToString()));
            }
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
