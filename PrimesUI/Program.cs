using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using DidasUtils.Logging;
using DidasUtils.Numerics;

using Raylib_cs;

using Primes.UI.Render;
using Primes.UI.Windows;

namespace Primes.UI
{
    static class Program
    {
        public static Font MonospaceFont { get; private set; }

        private static bool masterRun = true;
        private static Menu selectedMenu = Menu.Control;
        private static List<IRenderable> UI;

        #region Menus
        private static ControlWindow controlWindow;
        private static TestingWindow testingWindow;
        private static FilesWindow filesWindow;
        private static SettingsWindow settingsWindow;
        #endregion



        static void Main(string[] args)
        {
            try
            {
                if (!Init(args)) return;

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
                Log.LogException("Fatal unhandled exception.", "Main", e);
                Environment.Exit(1);
            }
        }



        private static bool Init(string[] args)
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

            //TODO: Parse args

            if (!InitWindow()) return false;
            if (!InitFont()) return false;
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
        private static bool InitFont()
        {
            string path = "./Fonts/courier-prime/Courier Prime Bold.ttf";
            if (!File.Exists(path)) return false;
            MonospaceFont = Raylib.LoadFont(path);

            return true;
        }
        private static bool InitUI()
        {
            UI = new List<IRenderable>();

            Holder pageHld = BuildMenuBar();

            controlWindow = new(); pageHld.Add(controlWindow.Window);
            testingWindow = new(); pageHld.Add(testingWindow.Window);
            filesWindow = new(); pageHld.Add(filesWindow.Window);
            settingsWindow = new(); pageHld.Add(settingsWindow.Window);

            return true;
        }



        private static void Update()
        {
            if (OpenPopups.Count == 0)
            {
                foreach (IRenderable rend in UI)
                {
                    if (rend is IUpdatable upd) upd.Update(Vector2i.Zero);
                }
            }
            else
                OpenPopups.Peek().Update(Vector2i.Zero);
        }
        private static void Draw()
        {
            foreach (IRenderable rend in UI)
            {
                rend.Render(Vector2i.Zero);
            }

            foreach (Holder holder in OpenPopups.Reverse())
                holder.Render(Vector2i.Zero);
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
        #endregion


        #region Popups
        public static readonly Stack<Holder> OpenPopups = new();
        //normal popups should be 400x250, cornered at 200x175
        //info/error popups should be 250x125, cornered at 300x

        public static void ShowPopup(Holder pop) => OpenPopups.Push(pop);
        public static void PopupUnhandledException(Exception e)
        {
            Button btn;
            Holder pop = new(new(200, 175));
            pop.Add(new Panel(new(0, 0), new(400, 250), new Color(200, 0, 0, 255)));
            pop.Add(new TextBox("An unhandled exception occurred!", new(20, 12), new(360, 28)));
            pop.Add(new TextBox("Please contact the developer.", new(20, 42), new(360, 28)));
            pop.Add(new TextBox($"Exception type: {e.GetType().ToString().Replace("System.", string.Empty)}", new(20, 92), new(360, 28)));
            pop.Add(btn = new("Close", new(172, 212), new(56, 26))); btn.OnPressed = OnPopupClosePressed;

            OpenPopups.Push(pop);
        }
        public static void PopupErrorMessage(string message)
        {
            Button btn;
            Holder pop = new(new(250, 230));
            pop.Add(new Panel(new(0, 0), new(300, 120), new Color(150, 50, 50, 255)));
            pop.Add(new TextBox("Error", new(10, 10), new(280, 20)));
            pop.Add(new TextBox(message, new(10, 35), new(280, 20)));
            pop.Add(btn = new("Close", new(122, 90), new(56, 25))); btn.OnPressed = OnPopupClosePressed;

            OpenPopups.Push(pop);
        }
        public static void PopupStatusMessage(string title, string message)
        {
            Button btn;
            Holder pop = new(new(250, 230));
            pop.Add(new Panel(new(0, 0), new(300, 120), new Color(50, 50, 50, 255)));
            pop.Add(new TextBox(title, new(10, 10), new(280, 20)));
            pop.Add(new TextBox(message, new(10, 35), new(280, 20)));
            pop.Add(btn = new("Close", new(122, 90), new(56, 25))); btn.OnPressed = OnPopupClosePressed;

            OpenPopups.Push(pop);
        }
        public static void PopupChoiceMessage(string title, string opt1, string opt2, EventHandler<int> choiceCallback)
        {
            //0 = aborted
            //1 = first
            //2 = second

            Button btn;
            Holder pop = new(new(250, 230));
            pop.Add(new Panel(new(0, 0), new(300, 120), new Color(50, 50, 50, 255)));
            pop.Add(new TextBox(title, new(10, 10), new(280, 20)));
            pop.Add(btn = new(opt1, new(10, 35), new(135, 20))); btn.OnPressed += (object sender, EventArgs _) => PopupChoicePressed(sender, 1, choiceCallback);
            pop.Add(btn = new(opt2, new(155, 35), new(135, 20))); btn.OnPressed += (object sender, EventArgs _) => PopupChoicePressed(sender, 2, choiceCallback);
            pop.Add(btn = new("Close", new(122, 90), new(56, 25))); btn.OnPressed += (object sender, EventArgs _) => PopupChoicePressed(sender, 0, choiceCallback);

            OpenPopups.Push(pop);
        }
        public static void OnPopupClosePressed(object sender, EventArgs e) => OpenPopups.Pop();
        public static void ClosePopup() => OpenPopups.Pop();
        public static Holder GetOpenPopup() => OpenPopups.Count != 0 ? OpenPopups.Peek() : null;


        private static void PopupChoicePressed(object sender, int code, EventHandler<int> callback)
        {
            ClosePopup();
            callback?.Invoke(sender, code);
        }
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


        #region Utils
        private static void EnableOnlyMenu(Menu menu)
        {
            foreach (Holder hld in ((Holder)UI.First((IRenderable rend) => rend is Holder)).Children.Cast<Holder>())
            {
                hld.Enabled = menu.ToString() == hld.Id;
            }
        }
        #endregion


        #region UIBuilding
        private static Holder BuildMenuBar()
        {
            Button btn; Holder hld;

            UI.Add(new Panel(Vector2i.Zero, new(800, 30), BaseWindow.Mid));

            UI.Add(btn = new("Control", new(2, 2), new(96, 26))); btn.OnPressed += OnMenuControlPressed;
            UI.Add(btn = new("Testing", new(102, 2), new(96, 26))); btn.OnPressed += OnMenuTestingPressed;
            UI.Add(btn = new("Files", new(202, 2), new(96, 26))); btn.OnPressed += OnMenuFilesPressed;
            UI.Add(btn = new("Stats", new(302, 2), new(96, 26))); btn.OnPressed += OnMenuStatsPressed;
            UI.Add(btn = new("Tools", new(402, 2), new(96, 26))); btn.OnPressed += OnMenuToolsPressed;
            UI.Add(btn = new("Settings", new(502, 2), new(96, 26))); btn.OnPressed += OnMenuSettingsPressed;
            UI.Add(btn = new("Exit", new(702, 2), new(96, 26))); btn.OnPressed += OnMenuExitPressed;

            UI.Add(hld = new Holder(new(0, 30))); hld.Id_Name = "PAGE_HOLDER";

            return hld;
        }
        #endregion
    }
}