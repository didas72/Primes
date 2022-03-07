using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using DidasUtils.Logging;
using DidasUtils.Numerics;

using Primes.Common;

using Raylib_cs;

using Primes.UI.Render;

namespace Primes.UI
{
    class Program
    {
        private static bool masterRun = true;
        private static Menu selectedMenu = Menu.Control;
        private static List<IRenderable> UI;

        #region Palette
        /*
         * Palette:
         * Background   51, 51, 51
         * Mid          77, 77, 77
         * Foreground   102, 102, 102
         * Text         0, 0, 0
         * Highlights   0, 206, 255
         */

        private static Color /*Background = new(51, 51, 51, 255),*/ Mid = new(77, 77, 77, 255), /*Foreground = new(102, 102, 102, 255), Text = new(0, 0, 0, 255),*/ Highlights = new(0, 206, 255, 255);

        #endregion



        static void Main(string[] args)
        {
            try
            {
                if (!Init()) return;

                //pass args

                EnableOnlyMenu(selectedMenu);

                while (masterRun && !Raylib.WindowShouldClose())
                {
                    try
                    {
                        Update();

                        Raylib.BeginDrawing();
                        Raylib.ClearBackground(Color.BLACK);
                        Draw();
                        Raylib.EndDrawing();
                    }
                    catch (Exception e)
                    {
                        Log.LogException("Unhandled exception.", "Main", e);
                        PopupUnhandledException(e);
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogException("Fatal nhandled exception.", "Main", e);
                Environment.Exit(1);
            }
        }



        private static bool Init()
        {
            Log.InitConsole();
            Log.UsePrint = false;

            try
            {
                string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes");
                Directory.CreateDirectory(logPath);
                Log.InitLog(logPath, "UI_Log.txt");
            }
            catch (Exception e)
            {
                Log.Print($"Failed to init: {e}");
                return false;
            }

            if (!InitWindow()) return false;
            if (!InitUI()) return false;

            return true;
        }
        private static bool InitWindow()
        {
            try
            {
                Raylib.InitWindow(800, 600, "Primes UI");
                Raylib.SetTargetFPS(30);

                return true;
            }
            catch (Exception e)
            {
                Log.LogException("Exception initting window.", "Init", e);
                return false;
            }
        }
        private static bool InitUI()
        {
            UI = new List<IRenderable>();
            Panel pnl; Button btn; TextBox txtBox; Holder pageHld, hld, hld1, hld2; ProgressBar prgBar; TextList txtLst;

            #region Menu
            pnl = new(Vector2i.Zero, new(800, 30), Mid);
            UI.Add(pnl);

            btn = new("Control", new(2, 2), new(96, 26));
            btn.OnPressed += OnMenuControlPressed;
            UI.Add(btn);

            btn = new("Testing", new(102, 2), new(96, 26));
            btn.OnPressed += OnMenuTestingPressed;
            UI.Add(btn);

            btn = new("Files", new(202, 2), new(96, 26));
            btn.OnPressed += OnMenuFilesPressed;
            UI.Add(btn);

            btn = new("Stats", new(302, 2), new(96, 26));
            btn.OnPressed += OnMenuStatsPressed;
            UI.Add(btn);

            btn = new("Tools", new(402, 2), new(96, 26));
            btn.OnPressed += OnMenuToolsPressed;
            UI.Add(btn);

            btn = new("Settings", new(502, 2), new(96, 26));
            btn.OnPressed += OnMenuSettingsPressed;
            UI.Add(btn);

            btn = new("Exit", new(702, 2), new(96, 26));
            btn.OnPressed += OnMenuExitPressed;
            UI.Add(btn);

            pageHld = new(new(0,30));
            UI.Add(pageHld);
            #endregion

            #region Control Menu
            hld = new(Vector2i.Zero, "Control");
            pageHld.Add(hld);

            btn = new("Connect local", new(2, 2), new(148, 28));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("Connect remote", new(152, 2), new(168, 28));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("Start/Stop", new(2, 32), new(148, 28));
            //btn.OnPressed += ;
            hld.Add(btn);

            txtBox = new("Not connected", 20, new(2,62), new(268, 28), Highlights);
            hld.Add(txtBox);

            prgBar = new(new(2, 92), new(298, 28));
            hld.Add(prgBar);

            txtBox = new("Batch XXXX", 20, new(302, 92), new(98, 28), Highlights);
            hld.Add(txtBox);

            btn = new("Open primes folder", new(2, 122), new(198, 28));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("Check for resource update", new(2, 152), new(298, 28));
            //btn.OnPressed += ;
            hld.Add(btn);
            #endregion

            #region Testing Menu
            hld = new(Vector2i.Zero, "Testing");
            pageHld.Add(hld);

            hld1 = new(Vector2i.Zero);
            hld.Add(hld1);
            hld2 = new(new(401, 0));//after divider
            hld.Add(hld2);

            //divider
            pnl = new(new(399, 0), new(2, 570), Mid);
            hld.Add(pnl);

            //=================
            //benchmarking side
            //=================
            txtBox = new("Benchmark", 30, new(4, 4), new(391, 42), Highlights);
            hld1.Add(txtBox);

            btn = new("Single-threaded", new(2, 52), new(166, 28));
            //btn.OnPressed += ;
            hld1.Add(btn);

            btn = new("Multi-threaded", new(172, 52), new(166, 28));
            //btn.OnPressed += ;
            hld1.Add(btn);

            txtBox = new("Benchmark status...", 20, new(2, 82), new(296, 28), Highlights);
            hld1.Add(txtBox);

            prgBar = new(new(2, 112), new(296, 28));
            hld1.Add(prgBar);

            txtBox = new("Score: XXX,XXX.XXX", 20, new(2, 142), new(196, 26), Highlights);
            hld1.Add(txtBox);

            txtLst = new(new(2,172), new(392, 60));
            hld1.Add(txtLst);

            //===================
            //stress testing side
            //===================
            txtBox = new("Stress Test", 30, new(4, 4), new(391, 42), Highlights);
            hld2.Add(txtBox);

            //TODO: input field
            txtBox = new("PLACEHOLDER", 20, new(2, 52), new(166, 26), Color.RED);
            hld2.Add(txtBox);

            btn = new("Start/Stop", new(2, 82), new(166, 28));
            //btn.OnPressed += ;
            hld2.Add(btn);

            txtBox = new("Test status...", 20, new(2, 112), new(296, 28), Highlights);
            hld2.Add(txtBox);

            prgBar = new(new(2, 142), new(296, 28));
            hld2.Add(prgBar);

            txtBox = new("CPU temp: XXX.X ºC", 20, new(2,172), new(296, 26), Highlights);
            hld2.Add(txtBox);
            #endregion

            #region Files Menu
            hld = new(Vector2i.Zero, "Files");
            pageHld.Add(hld);

            //File control
            pnl = new(new(0, 0), new(400, 30), Mid);
            hld.Add(pnl);

            btn = new("Open", new(2, 2), new(96, 26));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("New job", new(102, 2), new(96, 26));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("New rsrc", new(202, 2), new(96, 26));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("Save", new(302, 2), new(96, 26));
            //btn.OnPressed += ;
            hld.Add(btn);


            //View area
            txtLst = new(new(2, 32), new(396, 496));
            hld.Add(txtLst);

            btn = new("Switch view", new(2, 532), new(146, 26));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("Find...", new(152, 532), new(146, 26));
            //btn.OnPressed += ;
            hld.Add(btn);


            //Header area
            txtBox = new("Header", 20, new(402, 2), new(96, 26), Highlights);
            hld.Add(txtBox);

            txtLst = new(new(402, 32), new(396, 376));
            hld.Add(txtLst);

            btn = new("Change version", new(402, 412), new(176, 26));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("Change compression", new(582, 412), new(216, 26));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("Change field", new(402, 442), new(176, 26));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("Apply changes", new(582, 442), new(216, 26));
            //btn.OnPressed += ;
            hld.Add(btn);


            //Tools area
            txtBox = new("Tools", 20, new(402, 472), new(196, 26), Highlights);
            hld.Add(txtBox);

            btn = new("Validate", new(402, 502), new(96, 26));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("Fix", new(502, 502), new(96, 26));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("Convert", new(402, 532), new(96, 26));
            //btn.OnPressed += ;
            hld.Add(btn);

            btn = new("Export", new(502, 532), new(96, 26));
            //btn.OnPressed += ;
            hld.Add(btn);
            #endregion

            return true;
        }



        private static void Update()
        {
            foreach (IRenderable rend in UI)
            {
                if (rend is IUpdatable upd) upd.Update(Vector2i.Zero);
            }

            openPopup?.Update(Vector2i.Zero);
        }

        private static void Draw()
        {
            foreach (IRenderable rend in UI)
            {
                rend.Render(Vector2i.Zero);
            }

            openPopup?.Render(Vector2i.Zero);
        }



        #region Button Handles
        #region Menu Button Handles
        private static void OnMenuControlPressed(object sender, EventArgs e)
        {
            selectedMenu = Menu.Control;
            EnableOnlyMenu(selectedMenu);
        }
        private static void OnMenuTestingPressed(object sender, EventArgs e)
        {
            selectedMenu = Menu.Testing;
            EnableOnlyMenu(selectedMenu);
        }
        private static void OnMenuFilesPressed(object sender, EventArgs e)
        {
            selectedMenu = Menu.Files;
            EnableOnlyMenu(selectedMenu);
        }
        private static void OnMenuStatsPressed(object sender, EventArgs e)
        {
            selectedMenu = Menu.Stats;
            EnableOnlyMenu(selectedMenu);
        }
        private static void OnMenuToolsPressed(object sender, EventArgs e)
        {
            selectedMenu = Menu.Tools;
            EnableOnlyMenu(selectedMenu);
        }
        private static void OnMenuSettingsPressed(object sender, EventArgs e)
        {
            selectedMenu = Menu.Settings;
            EnableOnlyMenu(selectedMenu);
        }
        private static void OnMenuExitPressed(object sender, EventArgs e)
        {
            masterRun = false;
        }
        #endregion

        #region Control Menu Button Handles

        #endregion
        #endregion


        #region Popups
        #region General Popups
        private static Holder openPopup = null;
        //popups should be 400x250, cornered at 200x175

        private static void PopupUnhandledException(Exception e)
        {
            Holder pop = new(new(200, 175));
            pop.Add(new Panel(new(0, 0), new(400, 250), new Color(200, 0, 0, 255)));
            pop.Add(new TextBox("An unhandled exception occurred!", new(20, 12), new(360, 28)));
            pop.Add(new TextBox("Please contact the developer.", new(20, 42), new(360, 28)));
            pop.Add(new TextBox($"Exception type: {e.GetType().ToString().Replace("System.", string.Empty)}", new(20, 92), new(360, 28)));
            Button close = new("Close", new(172, 212), new(56, 26));
            close.OnPressed += PopupClosePressed;
            pop.Add(close);

            openPopup = pop;
        }

        private static void PopupClosePressed(object sender, EventArgs e)
        {
            openPopup = null;
        }
        #endregion
        #endregion



        #region Enums
        private enum Menu
        {
            Control,
            Testing,
            Files,
            Stats,
            Tools,
            Settings
        }
        #endregion



        private static void EnableOnlyMenu(Menu menu)
        {
            foreach (Holder hld in ((Holder)UI.First((IRenderable rend) => rend is Holder)).Children)
            {
                hld.Enabled = menu.ToString() == hld.Id;
            }
        }
    }
}