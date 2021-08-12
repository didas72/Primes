using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes.Exec
{
    public static class ConsoleUI
    {
        private static ushort progressBarWidth = 0;
        private static float progressBarMultiplier = 0f;

        private static readonly List<string> logLines = new List<string>();

        private static ushort maxLogLines = 0;

        private static Thread UIThread;
        private static volatile bool doUI = false;

        private static ushort[] lastJobSeconds;

        private static int lastWidth = 0, lastHeight = 0;

        private const string percentageFormat = "000.0";

        public static int frameTime = Mathf.Clamp(Properties.Settings.Default.FrameTimeMilis, 200, 60000);



        public static void StartUI()
        {
            doUI = true;

            lastJobSeconds = new ushort[Properties.Settings.Default.Threads];

            CalculateGraphicMetrics();

            UIThread = new Thread(UIWork);
            UIThread.Start();
        }
        public static void StopUI()
        {
            doUI = false;
        }

        

        private static void UIWork()
        {
            while (doUI)
            {
                DateTime start = DateTime.Now;

                string ui = "Primes.exe by Didas72 and PeakRead\n";

                if (lastWidth != Console.WindowWidth || lastHeight != Console.WindowHeight)
                {
                    lastWidth = Console.WindowWidth;
                    lastHeight = Console.WindowHeight;

                    CalculateGraphicMetrics();
                }

                DrawIndividualProgress(ref ui);
                DrawGlobalProgress(ref ui);
                DrawLog(ref ui);

                Console.Clear();
                Console.Write(ui);

                TimeSpan elapsed = DateTime.Now - start;

                Thread.Sleep(Math.Max(1, Math.Min(frameTime - elapsed.Milliseconds, frameTime)));
            }
        }



        private static void DrawIndividualProgress(ref string ui)
        {
            try
            {
                for (int i = 0; i < Program.jobDistributer.Workers.Length; i++)
                {
                    float progress = Program.jobDistributer.Workers[i].Progress;
                    uint batch = Program.jobDistributer.Workers[i].CurrentBatch;

                    int filledBarChars = (int)Math.Round(progress * progressBarMultiplier);

                    ui += $"#{i:D2} [{"=".Loop(filledBarChars)}{" ".Loop(progressBarWidth - filledBarChars)}] {progress.ToString(percentageFormat)}% Batch: {batch}\n";
                }
            }
            catch (Exception e)
            {
                Program.LogEvent(Program.EventType.Error, $"Error when drawing individual progress: {e.Message}", "ConsoleUI", false);
            }
        }
        private static void DrawGlobalProgress(ref string ui)
        {
            try
            {
                uint currentBatch = GetCurrentBatch();

                Directory.CreateDirectory(Path.Combine(Program.completePath, $"{currentBatch}"));

                int left = Directory.GetFiles(Path.Combine(Program.jobsPath, $"{currentBatch}\\"), "*.primejob", SearchOption.TopDirectoryOnly).Length;
                int done = Directory.GetFiles(Path.Combine(Program.completePath, $"{currentBatch}\\"), "*.primejob", SearchOption.TopDirectoryOnly).Length;

                int total = left + done + GetWorkersWorkingOnBatch(currentBatch);
                float progress = ((float)done * 100f) / (float)total;

                int filledBarChars = (int)Math.Round(progress * progressBarMultiplier);

                ushort avgJobSeconds = CalculateAverageJobSeconds();
                ulong ETRSeconds = (avgJobSeconds * (ulong)left) / Properties.Settings.Default.Threads;

                ui += $"Gbl [{"=".Loop(filledBarChars)}{" ".Loop(progressBarWidth - filledBarChars)}] {progress.ToString(percentageFormat)}% ETR: {(ETRSeconds / 3600):D2}h{((ETRSeconds / 60) % 60):D2}\n";
            }
            catch (Exception e)
            {
                Program.LogEvent(Program.EventType.Error, $"Failed to determine batch progress: {e.Message}", "ConsoleUI", false);
            }
        }
        private static void DrawLog(ref string ui)
        {
            try
            {
                ui += $"{"=".Loop(Console.BufferWidth)}Logs:\n";

                lock (logLines)
                {
                    for (int i = 0; i < maxLogLines && i < logLines.Count; i++)
                    {
                        ui += $"{logLines[logLines.Count - i - 1].SetLength(Console.BufferWidth)}";
                    }
                }
            }
            catch (Exception e)
            {
                Program.LogEvent(Program.EventType.Error, $"Error when drawing log: {e.Message}", "ConsoleUI", false);
            }
        }


        
        private static ushort CalculateAverageJobSeconds()
        {
            ushort avg = 0;

            foreach (ushort u in lastJobSeconds)
                avg += u;

            return (ushort)(avg / (float)lastJobSeconds.Length);
        }
        private static uint GetCurrentBatch()
        {
            return Program.jobDistributer.Workers[0].CurrentBatch;
        }
        private static int GetWorkersWorkingOnBatch(uint batch)
        {
            int count = 0;

            foreach (Worker worker in Program.jobDistributer.Workers)
                if (worker.CurrentBatch == batch)
                    count++;

            return count;
        }



        private static void CalculateGraphicMetrics()
        {
            progressBarWidth = (ushort)Mathf.Clamp((Console.WindowWidth - 27), 0, 200); //2 for [] to enclose bar; 3 for worker number (#XX); 1 ' '; 6 for percentage (XXX.X%); 1 ' '; 10 for batch (Batch: XXX); 4 ' ' for looks and to fit ETR
            progressBarMultiplier = progressBarWidth / 100f;

            maxLogLines = (ushort)Mathf.Clamp(Console.WindowHeight - Properties.Settings.Default.Threads - 5, 0, 50); //1 for title (Primes.exe by Didas72 and PeakRead); 1 for batch progress; 2 for divider + 'Logs:'; 1 for last line (always empty)
        }



        public static void AddLog(string message)
        {
            lock (logLines)
            {
                logLines.Add(message.TrimEnd('\n'));

                if (logLines.Count > 100)
                    logLines.RemoveAt(0);
            }
        }
        public static void RegisterJobSeconds(int threadID, double seconds)
        {
            lastJobSeconds[threadID] = (ushort)Math.Round(seconds);
        }
    }
}
