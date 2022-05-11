using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using DidasUtils.Logging;
using DidasUtils.Numerics;

using Raylib_cs;

using Primes.Common.Net;
using Primes.UI.Render;

namespace Primes.UI
{
    class Program
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



        static void Main(string[] args)
        {
            try
            {
                if (!Init()) return;

                //parse args

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

            BuildControlMenu(pageHld);
            BuildTestingMenu(pageHld);
            BuildFilesMenu(pageHld);

            ConnectionData.OnConnectionCheck += OnConnectionCheck;

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
            if (openPopups.Count == 0)
            {
                foreach (IRenderable rend in UI)
                {
                    if (rend is IUpdatable upd) upd.Update(Vector2i.Zero);
                }
            }
            else
                openPopups.Peek().Update(Vector2i.Zero);
        }

        private static void Draw()
        {
            foreach (IRenderable rend in UI)
            {
                rend.Render(Vector2i.Zero);
            }

            foreach (Holder holder in openPopups.Reverse())
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

        #region Control Menu Button Handles
        private static void OnControlRemoteConnectPressed(object sender, EventArgs e)
        {
            PopupControlConnectRemote();
        }
        private static void OnControlLocalConnectPressed(object sender, EventArgs e)
        {
            ConnectionData.RemoteEndpoint = new IPEndPoint(IPAddress.Loopback, 13031);

            ConnectionData.EnableCheckTimer();
            UpdateConnectionStatus();
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
                PopupErrorMessage("Failed to open file!"); //TODO: Error popup
            }
        }
        #endregion

        #region Popup Button Handles
        private static void OnConnectRemotePressed(object sender, EventArgs e)
        {
            if (!ValidateRemoteAddress()) return;

            UpdateConnectionStatus();
            ConnectionData.EnableCheckTimer();

            ClosePopup();
        }



        private static void OnPopupFileOpenPressed(Action<string, string> onClose)
        {
            TextList lst = openPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "DIR_LISTING") as TextList;
            TextBox txt = openPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

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
            TextList lst = openPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "DIR_LISTING") as TextList;
            TextBox txt = openPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

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
            TextBox txt = openPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

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
        private static readonly Stack<Holder> openPopups = new();
        //normal popups should be 400x250, cornered at 200x175
        //info/error popups should be 250x125, cornered at 300x

        #region General Popups
        private static void PopupUnhandledException(Exception e)
        {
            Button btn;
            Holder pop = new(new(200, 175));
            pop.Add(new Panel(new(0, 0), new(400, 250), new Color(200, 0, 0, 255)));
            pop.Add(new TextBox("An unhandled exception occurred!", new(20, 12), new(360, 28)));
            pop.Add(new TextBox("Please contact the developer.", new(20, 42), new(360, 28)));
            pop.Add(new TextBox($"Exception type: {e.GetType().ToString().Replace("System.", string.Empty)}", new(20, 92), new(360, 28)));
            pop.Add(btn = new("Close", new(172, 212), new(56, 26))); btn.OnPressed = OnPopupClosePressed;

            openPopups.Push(pop);
        }
        private static void PopupErrorMessage(string message)
        {
            Button btn;
            Holder pop = new(new(250, 230));
            pop.Add(new Panel(new(0, 0), new(300, 120), new Color(150, 50, 50, 255)));
            pop.Add(new TextBox("Error", new(10, 10), new(280, 20)));
            pop.Add(new TextBox(message, new(10, 35), new(280, 20)));
            pop.Add(btn = new("Close", new(122, 90), new(56, 25))); btn.OnPressed = OnPopupClosePressed;

            openPopups.Push(pop);
        }
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

            openPopups.Push(pop);

            FileOpenUpdate(filter, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }



        private static void OnPopupClosePressed(object sender, EventArgs e) => openPopups.Pop();
        private static void ClosePopup() => openPopups.Pop();
        #endregion

        #region Control Popups
        private static void PopupControlConnectRemote()
        {
            InputField field;
            Button btn;
            Holder pop = new(new(200, 175));
            pop.Add(new Panel(new(0, 0), new(400, 250), new Color(100, 100, 100, 255)));
            pop.Add(new TextBox("IP Address:", new(20, 20), new(200,20)));
            pop.Add(field = new InputField(new(20, 45), new(300,30))); field.Id_Name = "IP"; field.Text = "127.0.0.1:13031";
            pop.Add(btn = new("Connect", new(100, 212), new(90, 26))); btn.OnPressed += OnConnectRemotePressed;
            pop.Add(btn = new("Close", new(210, 212), new(90, 26))); btn.OnPressed += OnPopupClosePressed;

            openPopups.Push(pop);
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

            openPopups.Push(pop);
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


        #region Net Helper
        private static bool ValidateRemoteAddress()
        {
            if (!IPEndPoint.TryParse(((InputField)openPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "IP")).Text, out IPEndPoint endpoint))
            {
                PopupErrorMessage("Invalid IP and/or port.");
                return false;
            }

            ConnectionData.RemoteEndpoint = endpoint;

            return true;
        }
        private static void UpdateConnectionStatus()
        {
            string newStatus = string.Empty;

            try
            {
                if (!PingService(new TimeSpan(0, 0, 0, 0, 300)))
                    newStatus = "Failed to connect.";
                else
                    newStatus = "Connected.";
            }
            catch
            {
                newStatus = "Failed to connect.";
            }

            Holder pageHld = UI.Find((IRenderable rend) => rend.Id_Name == "PAGE_HOLDER") as Holder;
            Holder control = pageHld.Children.Find((IRenderable rend) => rend.Id_Name == "CONTROL") as Holder;
            TextBox connStatus = control.Children.Find((IRenderable rend) => rend.Id_Name == "CONNECTION_STATUS") as TextBox;
            connStatus.Text = newStatus;
        }
        private static void OnConnectionCheck(object sender, EventArgs e)
        {
            try
            {
                TimeSpan timeout = new(0, 0, 0, 0, 300);
                UpdateConnectionStatus();
                UpdateBatchNumber(timeout);
                UpdateBatchProgress(timeout);
                //TODO: add other needed things
            }
            catch
            {
                ConnectionData.DisableCheckTimer();
            }
        }
        private static bool PingService(TimeSpan timeout)
        {
            byte[] ping = MessageBuilder.Ping();
            bool ret;

            try
            {
                TcpClient cli = new();
                var result = cli.BeginConnect(ConnectionData.RemoteEndpoint.Address, ConnectionData.RemoteEndpoint.Port, null, null);

                var success = result.AsyncWaitHandle.WaitOne(timeout);
                MessageBuilder.SendMessage(ping, cli);

                ret = MessageBuilder.ReceiveMessage(cli.GetStream(), out byte[] _, timeout);
                cli.Close();

                return ret;
            }
            catch
            {
                return false;
            }
        }
        private static void UpdateBatchNumber(TimeSpan timeout)
        {
            byte[] message = MessageBuilder.Message("req", string.Empty, "cbnum");
            Holder pageHld = UI.Find((IRenderable rend) => rend.Id_Name == "PAGE_HOLDER") as Holder;
            Holder control = pageHld.Children.Find((IRenderable rend) => rend.Id_Name == "CONTROL") as Holder;
            TextBox connStatus = control.Children.Find((IRenderable rend) => rend.Id_Name == "BATCH_NUMBER") as TextBox;

            try
            {
                TcpClient cli = new();
                cli.Connect(ConnectionData.RemoteEndpoint);
                MessageBuilder.SendMessage(message, cli);

                if (!MessageBuilder.ReceiveMessage(cli.GetStream(), out message, timeout))
                {
                    connStatus.Text = "-1";
                    return;
                }

                MessageBuilder.DeserializeMessage(message, out string messageType, out string target, out object value);
                if (!MessageBuilder.ValidateReturnMessage(messageType, target, value) || !((string)value).Contains("REQUEST_PASS:"))
                {
                    connStatus.Text = "-1";
                    return;
                }

                connStatus.Text = ((string)value)[13..];
            }
            catch
            {
                connStatus.Text = "-1";
                return;
            }
        }
        private static void UpdateBatchProgress(TimeSpan timeout)
        {
            byte[] message = MessageBuilder.Message("req", string.Empty, "cbprog");
            Holder pageHld = UI.Find((IRenderable rend) => rend.Id_Name == "PAGE_HOLDER") as Holder;
            Holder control = pageHld.Children.Find((IRenderable rend) => rend.Id_Name == "CONTROL") as Holder;
            ProgressBar batchPrg = control.Children.Find((IRenderable rend) => rend.Id_Name == "BATCH_PROGRESS") as ProgressBar;

            try
            {
                TcpClient cli = new();
                cli.Connect(ConnectionData.RemoteEndpoint);
                MessageBuilder.SendMessage(message, cli);

                if (!MessageBuilder.ReceiveMessage(cli.GetStream(), out message, timeout))
                {
                    batchPrg.Value = 0f;
                    return;
                }

                MessageBuilder.DeserializeMessage(message, out string messageType, out string target, out object value);
                if (!MessageBuilder.ValidateReturnMessage(messageType, target, value) || !((string)value).Contains("REQUEST_PASS:"))
                {
                    batchPrg.Value = 0f;
                    return;
                }

                batchPrg.Value = float.Parse(((string)value)[13..]);
            }
            catch
            {
                batchPrg.Value = 0;
                return;
            }
        }
        #endregion


        private static void EnableOnlyMenu(Menu menu)
        {
            foreach (Holder hld in ((Holder)UI.First((IRenderable rend) => rend is Holder)).Children)
            {
                hld.Enabled = menu.ToString() == hld.Id;
            }
        }
        private static void FileOpenUpdate(string filter, string path)
        {
            TextList lst = openPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "DIR_LISTING") as TextList;
            TextBox txt = openPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

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
        private static void BuildControlMenu(Holder pageHld)
        {
            Holder hld; Button btn; TextBox txtBox; ProgressBar prgBar;
            pageHld.Add(hld = new Holder(Vector2i.Zero, "Control")); hld.Id_Name = "CONTROL";

            hld.Add(btn = new("Connect local", new(2, 2), new(148, 28))); btn.OnPressed += OnControlLocalConnectPressed;
            hld.Add(btn = new("Connect remote", new(152, 2), new(168, 28))); btn.OnPressed += OnControlRemoteConnectPressed;
            hld.Add(btn = new("Start/Stop", new(2, 32), new(148, 28))); //btn.OnPressed += ;
            hld.Add(txtBox = new TextBox("Not connected", 20, new(2, 62), new(268, 28), Highlights)); txtBox.Id_Name = "CONNECTION_STATUS";
            hld.Add(prgBar = new(new(2, 92), new(298, 28))); prgBar.Id_Name = "BATCH_PROGRESS";
            hld.Add(txtBox = new("Batch XXXX", 20, new(302, 92), new(98, 28), Highlights)); txtBox.Id_Name = "BATCH_NUMBER";
            hld.Add(btn = new("Open primes folder", new(2, 122), new(198, 28))); //btn.OnPressed += ;
            hld.Add(btn = new("Check for resource update", new(2, 152), new(298, 28))); //btn.OnPressed += ;
        }
        private static void BuildTestingMenu(Holder pageHld)
        {
            Holder hld, hld1, hld2; TextBox txtBox; Button btn; ProgressBar prgBar; TextList txtLst; InputField inp;
            pageHld.Add(hld = new(Vector2i.Zero, "Testing")); hld.Id_Name = "TESTING";

            hld.Add(hld1 = new(Vector2i.Zero, "Benchmarking")); hld1.Id_Name = "BENCHMARK";
            hld.Add(hld2 = new(new(401, 0), "Stress Testing")); hld2.Id_Name = "STRESSTEST";//after divider

            //divider
            hld.Add(new Panel(new(399, 0), new(2, 570), Mid));

            //=================
            //benchmarking side
            //=================
            hld1.Add(new TextBox("Benchmark", 30, new(4, 4), new(391, 42), Highlights));
            hld1.Add(btn = new("Single-threaded", new(2, 52), new(166, 28))); //btn.OnPressed += ;
            hld1.Add(btn = new("Multi-threaded", new(172, 52), new(166, 28))); //btn.OnPressed += ;
            hld1.Add(txtBox = new("Benchmark status...", 20, new(2, 82), new(296, 28), Highlights)); txtBox.Id_Name = "BENCHMARK_STATUS";
            hld1.Add(prgBar = new(new(2, 112), new(296, 28))); prgBar.Id_Name = "BENCHMARK_PROGRESS";
            hld1.Add(txtBox = new("Score: XXX,XXX.XXX", 20, new(2, 142), new(196, 26), Highlights)); txtBox.Id_Name = "BENCHMARK_SCORE";
            hld1.Add(txtLst = new(new(2, 172), new(392, 60))); txtLst.Id_Name = "BENCHMARK_HISTORY";

            //===================
            //stress testing side
            //===================
            hld2.Add(new TextBox("Stress Test", 30, new(4, 4), new(391, 42), Highlights));
            hld2.Add(new TextBox("Threads:", new(2, 52), new(95, 26)));
            hld2.Add(inp = new(new(97, 52), new(71, 26))); inp.Text = "4";
            hld2.Add(btn = new("Start/Stop", new(2, 82), new(166, 28))); //btn.OnPressed += ;
            hld2.Add(txtBox = new("Test status...", 20, new(2, 112), new(296, 28), Highlights)); txtBox.Id_Name = "STRESSTEST_STATUS";
            hld2.Add(prgBar = new(new(2, 142), new(296, 28))); prgBar.Id_Name = "STRESSTEST_PROGRESS";
            hld2.Add(txtBox = new("CPU temp: XXX.X ºC", 20, new(2, 172), new(296, 26), Highlights)); txtBox.Id_Name = "CPU_TEMP";
        }
        private static void BuildFilesMenu(Holder pageHld)
        {
            Button btn; TextBox txtBox; Holder hld; TextList lst;
            hld = new(Vector2i.Zero, "Files"); hld.Id_Name = "FILES";
            pageHld.Add(hld);

            //File control
            hld.Add(new Panel(new(0, 0), new(400, 30), Mid));
            hld.Add(btn = new("Open", new(2, 2), new(96, 26))); btn.OnPressed += OnFilesOpenPressed;
            hld.Add(btn = new("New job", new(102, 2), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.CreateNewJob();
            hld.Add(btn = new("New rsrc", new(202, 2), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.CreateNewJob();
            hld.Add(btn = new("Save", new(302, 2), new(96, 26))); //btn.OnPressed += ; //TODO: Save popup


            //View area
            hld.Add(lst = new(new(2, 32), new(396, 496))); lst.Id_Name = "VIEW_LIST"; FileHandler.SetContent(lst); lst.CustomFont = monospaceFont; lst.UseCustomFont = true;
            hld.Add(btn = new("Switch view", new(2, 532), new(146, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.SwitchView();
            hld.Add(btn = new("Find...", new(152, 532), new(146, 26))); //btn.OnPressed += ; //TODO: Find popup


            //Header area
            hld.Add(txtBox = new("Header", 20, new(402, 2), new(96, 26), Highlights));
            hld.Add(lst = new(new(402, 32), new(396, 376))); lst.Id_Name = "HEADER_LIST"; FileHandler.SetHeader(lst); lst.CustomFont = monospaceFont; lst.UseCustomFont = true;
            hld.Add(btn = new("Change version", new(402, 412), new(176, 26))); //btn.OnPressed += ; //TODO: 
            hld.Add(btn = new("Change compression", new(582, 412), new(216, 26))); //btn.OnPressed += ; //TODO: 
            hld.Add(btn = new("Change field", new(402, 442), new(176, 26))); //btn.OnPressed += ; //TODO: 
            hld.Add(btn = new("Apply changes", new(582, 442), new(216, 26))); //btn.OnPressed += ; //TODO: 


            //Tools area
            hld.Add(txtBox = new("Tools", 20, new(402, 472), new(196, 26), Highlights));
            hld.Add(btn = new("Validate", new(402, 502), new(96, 26))); //btn.OnPressed += ; //TODO: 
            hld.Add(btn = new("Fix", new(502, 502), new(96, 26))); //btn.OnPressed += ; //TODO: 
            hld.Add(btn = new("Convert", new(402, 532), new(96, 26))); //btn.OnPressed += ; //TODO: 
            hld.Add(btn = new("Export", new(502, 532), new(96, 26))); //btn.OnPressed += ; //TODO: 
        }
        #endregion
    }
}