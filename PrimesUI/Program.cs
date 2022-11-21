using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

using DidasUtils.Logging;
using DidasUtils.Numerics;

using Raylib_cs;

using Primes.Common.Net;
using Primes.UI.Render;
using Primes.UI.Windows;

namespace Primes.UI
{
    static class Program
    {
        private static bool masterRun = true;
        private static Menu selectedMenu = Menu.Control;
        private static List<IRenderable> UI;
        private static Font monospaceFont;

        #region Palette
        /*
         * Palette:
         * Background   51, 51, 51
         * Mid          77, 77, 77
         * Foreground   102, 102, 102
         * Text         0, 0, 0
         * Highlights   0, 206, 255
         */

        private static Color Background = new(51, 51, 51, 255), Mid = new(77, 77, 77, 255), Foreground = new(102, 102, 102, 255), Text = new(0, 0, 0, 255), Highlights = new(0, 206, 255, 255);
        #endregion

        #region Menus
        private static ControlWindow controlWindow;
        private static TestingWindow testingWindow;
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
        private static bool InitUI()
        {
            UI = new List<IRenderable>();

            Holder pageHld = BuildMenuBar();

            controlWindow = new(); pageHld.Add(controlWindow.Window);
            testingWindow = new(); pageHld.Add(testingWindow.Window);
            BuildFilesMenu(pageHld);

            return true;
        }
        private static bool InitFont()
        {
            string path = "./Fonts/courier-prime/Courier Prime Bold.ttf";
            if (!File.Exists(path)) return false;
            monospaceFont = Raylib.LoadFont(path);

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

        #region Files Menu Button Handles
        private static void OnFilesOpenPressed(object sender, EventArgs e)
        {
            PopupFilesOpen();
        }

        private static void OnPopupFilesOpenPressed(string status, string path)
        {
            if (status == "CANCEL") return;

            if (status == "INVALID")
            {
                PopupErrorMessage("Invalid path/file!");
                return;
            }

            if (status != "OK") throw new NotImplementedException();

            if (!FileHandler.Open(path))
            {
                PopupErrorMessage("Failed to open file!");
            }
        }
        #endregion

        #region Popup Button Handles
        private static void OnPopupFileOpenPressed(Action<string, string> onClose)
        {
            TextList lst = OpenPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "DIR_LISTING") as TextList;
            TextBox txt = OpenPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

            try
            {
                string path = Path.Combine(txt.Text, lst.Lines[lst.Selected]);
                ClosePopup();
                Log.LogEvent($"Selected path is {path}", "PopupFileOpenPressed");

                if (!File.Exists(path))
                {
                    Log.LogEvent(Log.EventType.Error, "File does not exists.", "PopupFileOpenPressed");
                    onClose("INVALID", string.Empty);
                }
                else
                    onClose("OK", path);
            }
            catch (Exception e) { Log.LogException("Failed to open file.", "PopupFileOpenPressed", e); onClose("INVALID", string.Empty); }
        }
        private static void OnPopupFileCancelPressed(Action<string, string> onClose)
        {
            onClose("CANCEL", string.Empty);
            ClosePopup();
        }
        private static void OnPopupFileSelectedPressed(string filter)
        {
            TextList lst = OpenPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "DIR_LISTING") as TextList;
            TextBox txt = OpenPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

            string path;
            string file = lst.Lines[lst.Selected];

            if (file.StartsWith('>')) //directory
            {
                path = Path.Combine(txt.Text, file.TrimStart('>'));
                FileOpenUpdate(filter, path);
            }

            //do nothing if file
        }
        private static void OnPopupFileUpPressed(string filter)
        {
            TextBox txt = OpenPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

            try
            {
                string path = Path.GetDirectoryName(txt.Text);
                FileOpenUpdate(filter, path);
            }
            catch
            {
                FileOpenUpdate(filter, string.Empty);
            }
        }



        private static void OnPopupFilesOpenOpenJobPressed(object sender, EventArgs e)
        {
            ClosePopup();
            PopupOpenFile("*.primejob", OnPopupFilesOpenPressed);
        }
        private static void OnPopupFilesOpenOpenResourcePressed(object sender, EventArgs e)
        {
            ClosePopup();
            PopupOpenFile("*.rsrc", OnPopupFilesOpenPressed);
        }
        #endregion
        #endregion


        #region Popups
        public static readonly Stack<Holder> OpenPopups = new();
        //normal popups should be 400x250, cornered at 200x175
        //info/error popups should be 250x125, cornered at 300x

        #region General Popups
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
        private static void PopupOpenFile(string filter, Action<string, string> onClose)
        {
            Button btn; TextList lst; TextBox txt;
            Holder pop = new(new(200, 175));
            pop.Add(new Panel(new(0, 0), new(400, 250), Background));
            pop.Add(new TextBox("Select file to open:", new(10, 10), new(280, 20)));
            pop.Add(txt = new TextBox("Path: ", new(40, 30), new(350, 20))); txt.Id_Name = "CURRENT_PATH";
            pop.Add(btn = new Button("^", new(10, 30), new(20, 20))); btn.OnPressed += (object sender, EventArgs e) => OnPopupFileUpPressed(filter);
            pop.Add(lst = TextList.CreateSelectable(new(10, 50), new(380, 175))); lst.Id_Name = "DIR_LISTING"; lst.OnSelected += (object sender, EventArgs e) => OnPopupFileSelectedPressed(filter);
            pop.Add(btn = new Button("Cancel", new(180, 220), new(100, 20))); btn.OnPressed += (object sender, EventArgs e) => OnPopupFileCancelPressed(onClose);
            pop.Add(btn = new Button("Open", new(290, 220), new(100, 20))); btn.OnPressed += (object sender, EventArgs e) => OnPopupFileOpenPressed(onClose);

            OpenPopups.Push(pop);

            FileOpenUpdate(filter, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }
        private static void PopupChoicePressed(object sender, int code, EventHandler<int> callback)
        {
            ClosePopup();
            callback?.Invoke(sender, code);
        }
        #endregion

        #region Files Popups
        private static void PopupFilesOpen()
        {
            Button btn;
            Holder pop = new(new(275, 260));
            pop.Add(new Panel(new(0, 0), new(250, 80), new Color(100, 100, 100, 255)));
            pop.Add(new TextBox("Open:", new(10, 10), new(200, 20)));
            pop.Add(btn = new("Job", new(10, 50), new(100, 20))); btn.OnPressed += OnPopupFilesOpenOpenJobPressed;
            pop.Add(btn = new("Resource", new(140, 50), new(100, 20))); btn.OnPressed += OnPopupFilesOpenOpenResourcePressed;

            OpenPopups.Push(pop);
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
            foreach (Holder hld in ((Holder)UI.First((IRenderable rend) => rend is Holder)).Children.Cast<Holder>())
            {
                hld.Enabled = menu.ToString() == hld.Id;
            }
        }
        private static void FileOpenUpdate(string filter, string path)
        {
            TextList lst = OpenPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "DIR_LISTING") as TextList;
            TextBox txt = OpenPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

            txt.Text = path;
            lst.Lines.Clear();

            if (path == string.Empty) //show drive list
            {
                foreach (string d in Environment.GetLogicalDrives())
                {
                    lst.Lines.Add(">" + d);
                }
            }
            else
            {
                string[] subDirs = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path, filter);

                lst.Scroll = 0;

                foreach (string dir in subDirs) //diff appearance for folders
                    lst.Lines.Add(">" + Path.GetFileName(dir));
                foreach (string file in files)
                    lst.Lines.Add(Path.GetFileName(file));
            }
        }



        #region UIBuilding
        private static Holder BuildMenuBar()
        {
            Button btn; Holder hld;

            UI.Add(new Panel(Vector2i.Zero, new(800, 30), Mid));

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
        private static Holder _FilesHolder;
        private static void BuildFilesMenu(Holder pageHld)
        {
            Button btn; TextBox txtBox; Holder hld; TextList lst;
            pageHld.Add(hld = new(Vector2i.Zero, "Files")); hld.Id_Name = "FILES"; _FilesHolder = hld;

            //File control
            hld.Add(new Panel(new(0, 0), new(400, 30), Mid));
            hld.Add(btn = new("Open", new(2, 2), new(96, 26))); btn.OnPressed += OnFilesOpenPressed;
            hld.Add(btn = new("New job", new(102, 2), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.CreateNewJob();
            hld.Add(btn = new("New rsrc", new(202, 2), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.CreateNewResource();
            hld.Add(btn = new("Save", new(302, 2), new(96, 26))); //btn.OnPressed += ; //TODO: Save popup


            //View area
            hld.Add(lst = new(new(2, 32), new(396, 496))); lst.Id_Name = "VIEW_LIST"; FileHandler.SetContent(lst); lst.CustomFont = monospaceFont; lst.UseCustomFont = true;
            hld.Add(btn = new("Switch view", new(2, 532), new(146, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.SwitchView();
            hld.Add(btn = new("Find...", new(152, 532), new(116, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.Find();
            hld.Add(btn = new("Go to...", new(272, 532), new(116, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.GoTo();


            //Header area
            hld.Add(txtBox = new("Header", 20, new(402, 2), new(96, 26), Highlights));
            hld.Add(lst = new(new(402, 32), new(396, 376))); lst.Id_Name = "HEADER_LIST"; FileHandler.SetHeader(lst); lst.CustomFont = monospaceFont; lst.UseCustomFont = true;
            hld.Add(btn = new("Change version", new(402, 412), new(176, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.ChangeVersion();
            hld.Add(btn = new("Change compression", new(582, 412), new(216, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.ChangeCompression();
            hld.Add(btn = new("Change field", new(402, 442), new(176, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.ChangeField();
            hld.Add(btn = new("Apply changes", new(582, 442), new(216, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.ApplyChanges();


            //Tools area
            hld.Add(txtBox = new("Tools", 20, new(402, 472), new(196, 26), Highlights));
            hld.Add(btn = new("Validate", new(402, 502), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.Validate();
            hld.Add(btn = new("Fix", new(502, 502), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.Fix();
            hld.Add(btn = new("Convert", new(402, 532), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.Convert();
            hld.Add(btn = new("Export", new(502, 532), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.Export();
        }
        #endregion
    }
}