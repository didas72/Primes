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
        private static ushort logStartLine = 0;
        private static volatile bool logsChanged = false;

        private static Thread UIThread;
        private static volatile bool doUI = false;

        private static ushort[] lastJobSeconds;
        private static uint UIFrame = 0;

        private static int lastWidth = 0, lastHeight = 0;

        private const string percentageFormat = "000.0";



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
            DrawUIBase();

            Thread.Sleep(500);

            while (doUI)
            {
                DateTime start = DateTime.Now;

                if (lastWidth != Console.WindowWidth || lastHeight != Console.WindowHeight)
                {
                    lastWidth = Console.WindowWidth;
                    lastHeight = Console.WindowHeight;

                    CalculateGraphicMetrics();
                    DrawUIBase();
                    DrawGlobalProgress();

                    logsChanged = true;
                }
                else if (UIFrame % 5 == 0)
                {
                    DrawGlobalProgress();
                }

                DrawIndividualProgress();
                DrawLog();

                TimeSpan elapsed = DateTime.Now - start;

                Thread.Sleep(Math.Max(1, 1000 - elapsed.Milliseconds)); //1000ms = every 1s

                UIFrame++;
            }
        }



        private static void DrawUIBase()
        {
            try
            {
                Console.Clear();

                SafeWriteAt(0, 0, "Primes.exe by Didas72 and PeakRead");

                for (int i = 0; i < Program.jobDistributer.Workers.Length; i++)
                {
                    SafeWriteAt(0, i + 1, $"#{i:D2} [");

                    SafeWriteAt(5 + progressBarWidth, i + 1, "] XXX.X% Batch: X");
                }

                SafeWriteAt(0, logStartLine - 3, "Gbl [");

                SafeWriteAt(5 + progressBarWidth, logStartLine - 3, "] XXX.X% ETR: XXhXXm");

                SafeWriteAt(0, logStartLine - 2, "=".Loop(Console.WindowWidth - 1));
                SafeWriteAt(0, logStartLine - 1, "Logs:");
            }
            catch (Exception e)
            {
                Program.LogEvent(Program.EventType.Error, $"Error when drawing UI base: {e.Message}", "ConsoleUI", false);
            }
        }
        private static void DrawIndividualProgress()
        {
            try
            {
                for (int i = 0; i < Program.jobDistributer.Workers.Length; i++)
                {
                    float progress = Program.jobDistributer.Workers[i].Progress;
                    uint batch = Program.jobDistributer.Workers[i].CurrentBatch;

                    int filledBarChars = (int)Math.Round(progress * progressBarMultiplier);

                    SafeWriteAt(5, i + 1, $"{"=".Loop(filledBarChars)}");

                    SafeWriteAt(7 + progressBarWidth, i + 1, progress.ToString(percentageFormat));

                    SafeWriteAt(21 + progressBarWidth, i + 1, batch.ToString());
                }
            }
            catch (Exception e)
            {
                Program.LogEvent(Program.EventType.Error, $"Error when drawing individual progress: {e.Message}", "ConsoleUI", false);
            }
        }
        private static void DrawGlobalProgress()
        {
            try
            {
                uint currentBatch = GetCurrentBatch();

                Directory.CreateDirectory(Path.Combine(Program.completePath, $"{currentBatch}"));

                int left = Directory.GetFiles(Path.Combine(Program.jobsPath, $"{currentBatch}\\"), "*.primejob").Length;
                int done = Directory.GetFiles(Path.Combine(Program.completePath, $"{currentBatch}\\"), "*.primejob").Length;

                int total = left + done + GetWorkersWorkingOnBatch(currentBatch);
                float progress = ((float)done * 100f) / (float)total;

                int filledBarChars = (int)Math.Round(progress * progressBarMultiplier);

                ushort avgJobSeconds = CalculateAverageJobSeconds();
                ulong ETRSeconds = (avgJobSeconds * (ulong)left) / Properties.Settings.Default.Threads;

                SafeWriteAt(5, logStartLine - 3, $"{"=".Loop(filledBarChars)}] {progress.ToString(percentageFormat)}% ETR: {(ETRSeconds / 3600):D2}h{((ETRSeconds / 60) % 60):D2}m");
            }
            catch (Exception e)
            {
                Program.LogEvent(Program.EventType.Error, $"Failed to determine batch progress: {e.Message}", "ConsoleUI", false);
            }
        }
        private static void DrawLog()
        {
            try
            {
                if (logsChanged)
                {
                    lock (logLines)
                    {
                        for (int i = 0; i < maxLogLines && i < logLines.Count; i++)
                        {
                            SafeWriteAt(0, logStartLine + i, logLines[logLines.Count - i - 1].SetLength(Console.WindowWidth));
                        }
                    }

                    logsChanged = false;
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
        private static void SafeWriteAt(int left, int top, string message)
        {
            try
            {
                Console.SetCursorPosition(Math.Min(Math.Max(Console.BufferWidth - 1, 0), left), Math.Min(Math.Max(Console.BufferHeight - 1, 0), top));

                Console.Write(message);
            }
            catch (Exception e)
            {
                Program.LogEvent(Program.EventType.Error, $"Error when writting at {left}, {top}: {e.Message}", "ConsoleUI", false);
            }
        }



        private static void CalculateGraphicMetrics()
        {
            progressBarWidth = (ushort)(Console.WindowWidth - 27); //2 for [] to enclose bar; 3 for worker number (#XX); 1 ' '; 6 for percentage (XXX.X%); 1 ' '; 10 for batch (Batch: XXX); 4 ' ' for looks and to fit ETR
            progressBarMultiplier = progressBarWidth / 100f;

            maxLogLines = (ushort)(Console.WindowHeight - Properties.Settings.Default.Threads - 5); //1 for title (Primes.exe by Didas72 and PeakRead); 1 for batch progress; 2 for divider + 'Logs:'; 1 for last line (always empty)
            logStartLine = (ushort)(Properties.Settings.Default.Threads + 4);
        }



        public static void AddLog(string message)
        {
            logsChanged = true;

            lock (logLines)
            {
                logLines.Add(message);

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
