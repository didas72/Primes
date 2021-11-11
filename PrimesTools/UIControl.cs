using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

using Microsoft.Win32;

using Primes.Common.Files;

namespace PrimesTools
{
    public static class UIControl
    {
        public static ListView Header, Primes, Binary, Stats;
        public static TextBlock Status;
        public static ProgressBar Progress;


        private static PrimeJob job;



        public static void Init(ListView header, ListView primes, ListView binary, ListView stats, TextBlock status, ProgressBar progress)
        {
            Header = header; Primes = primes; Binary = binary; Stats = stats; Status = status; Progress = progress;

            EmptyAllLists();
            SetStatus("Ready.");
            SetProgress(0d);
        }



        public static async Task OpenFile()
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                DefaultExt = "primejob",
                Filter = "PrimeJob files|*.primejob|All files|*.*",
                Title = "Choose prime job file",
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Multiselect = false,
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    SetStatus("Loading file...");

                    SetProgress(0d);
                    await LoadJob(dialog.FileName);
                    SetStatus("Drawing header...");

                    SetProgress(0.2d);
                    await DrawHeader();
                    SetStatus("Drawing primes...");

                    SetProgress(0.4d);
                    await DrawPrimes();
                    SetStatus("Drawing binary...");

                    SetProgress(0.6d);
                    await DrawBinary();
                    SetStatus("Drawing Stats...");

                    SetProgress(0.8d);
                    await DrawStats();
                    SetStatus("Done.");

                    SetProgress(1);
                }
                catch
                {
                    SetStatus("Error loading file.");
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
            SetStatus("File closed.");
        }



        private static async Task LoadJob(string path)
        {
            lock (job)
            {
                job = PrimeJob.Deserialize(path);
            }
        }
        private static async Task DrawHeader()
        {
            lock (Header)
            {
                Header.Items.Add(new ValueItemPair("Version", job.FileVersion.ToString()));
                Header.Items.Add(new ValueItemPair("Compression", job.FileCompression.GetByte().ToString("X2")));
                Header.Items.Add(new ValueItemPair("Batch", job.Batch.ToString()));
                Header.Items.Add(new ValueItemPair("Start", job.Start.ToString()));
                Header.Items.Add(new ValueItemPair("Count", job.Count.ToString()));
                Header.Items.Add(new ValueItemPair("Progress", job.Progress.ToString()));
                Header.Items.Add(new ValueItemPair("Status (Computed)", job.PeekStatus().ToString()));
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
    }
}
