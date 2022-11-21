using System;
using System.Collections.Generic;
using System.Linq;

using Raylib_cs;

using DidasUtils.Numerics;

using Primes.UI.Render;

namespace Primes.UI.Windows
{
    internal class TestingWindow : BaseWindow
    {
        private readonly TextBox BenchmarkStatusText, BenchmarkScoreText;
        private readonly ProgressBar BenchmarkProgressBar;
        private readonly TextList BenchmarkHistory;

        private readonly InputField StresstestThreadsInput;
        private readonly TextBox StresstestStatusText, StresstestCPUTempText;
        //private readonly ProgressBar StresstestProgressBar; //TODO: Really needed?


        public TestingWindow()
        {
            Holder benchmarkHld, stresstestHld; TextBox txtBox; Button btn; ProgressBar prgBar; TextList txtLst; InputField inp;

            Window = new(Vector2i.Zero, "Testing") { Id_Name = "TESTING" };

            Window.Add(benchmarkHld = new(Vector2i.Zero, "Benchmarking") { Id_Name = "BENCHMARK" });
            Window.Add(stresstestHld = new(new(401, 0), "Stress Testing") { Id_Name = "STRESSTEST" });

            //divider
            Window.Add(new Panel(new(399, 0), new(2, 570), Mid));

            //=================
            //benchmarking side
            //=================
            benchmarkHld.Add(new TextBox("Benchmark", 30, new(4, 4), new(391, 42), Highlights));
            benchmarkHld.Add(btn = new("Single-threaded", new(2, 52), new(166, 28))); btn.OnPressed += OnSingleThreadedPressed;
            benchmarkHld.Add(btn = new("Multi-threaded", new(172, 52), new(166, 28))); btn.OnPressed += OnMultiThreadedPressed;
            benchmarkHld.Add(txtBox = new("Benchmark status...", 20, new(2, 82), new(296, 28), Highlights)); txtBox.Id_Name = "BENCHMARK_STATUS"; BenchmarkStatusText = txtBox;
            benchmarkHld.Add(prgBar = new(new(2, 112), new(296, 28))); prgBar.Id_Name = "BENCHMARK_PROGRESS"; BenchmarkProgressBar = prgBar;
            benchmarkHld.Add(txtBox = new("Score: XXX,XXX.XXX", 20, new(2, 142), new(196, 26), Highlights)); txtBox.Id_Name = "BENCHMARK_SCORE"; BenchmarkScoreText = txtBox;
            benchmarkHld.Add(new TextBox("History:", 20, new(2, 172), new(296, 28), Highlights));
            benchmarkHld.Add(txtLst = new(new(2, 202), new(392, 366))); txtLst.Id_Name = "BENCHMARK_HISTORY"; BenchmarkHistory = txtLst;

            //===================
            //stress testing side
            //===================
            stresstestHld.Add(new TextBox("Stress Test", 30, new(4, 4), new(391, 42), Highlights));
            stresstestHld.Add(new TextBox("Threads:", new(2, 52), new(95, 26)));
            stresstestHld.Add(inp = new(new(97, 52), new(71, 26))); inp.Text = "4"; StresstestThreadsInput = inp;
            stresstestHld.Add(btn = new("Start/Stop", new(2, 82), new(166, 28))); btn.OnPressed += OnStressTestPressed;
            stresstestHld.Add(txtBox = new("Test status...", 20, new(2, 112), new(296, 28), Highlights)); txtBox.Id_Name = "STRESSTEST_STATUS"; StresstestStatusText = txtBox;
            stresstestHld.Add(prgBar = new(new(2, 142), new(296, 28))); prgBar.Id_Name = "STRESSTEST_PROGRESS"; //TODO: Is this really needed? Should be unlimited
            stresstestHld.Add(txtBox = new("CPU temp: XXX.X ºC", 20, new(2, 172), new(296, 26), Highlights)); txtBox.Id_Name = "CPU_TEMP"; StresstestCPUTempText = txtBox;
        }

        #region Button handles
        private void OnSingleThreadedPressed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        private void OnMultiThreadedPressed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        private void OnStressTestPressed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
