using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Raylib_cs;

using DidasUtils.Numerics;
using DidasUtils.Logging;

using Primes.UI.Render;

namespace Primes.UI.Windows
{
    internal class FilesWindow : BaseWindow
    {




        public FilesWindow()
        {
            Button btn; TextBox txtBox; TextList lst;
            Window = new(Vector2i.Zero, "Files") { Id_Name = "FILES" };

            //File control
            Window.Add(new Panel(new(0, 0), new(400, 30), Mid));
            Window.Add(btn = new("Open", new(2, 2), new(96, 26))); btn.OnPressed += OnFilesOpenPressed;
            Window.Add(btn = new("New job", new(102, 2), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.CreateNewJob();
            Window.Add(btn = new("New rsrc", new(202, 2), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.CreateNewResource();
            Window.Add(btn = new("Save", new(302, 2), new(96, 26))); //btn.OnPressed += ; //TODO: Save popup


            //View area
            Window.Add(lst = new(new(2, 32), new(396, 496))); lst.Id_Name = "VIEW_LIST"; FileHandler.SetContent(lst); lst.CustomFont = Program.MonospaceFont; lst.UseCustomFont = true;
            Window.Add(btn = new("Switch view", new(2, 532), new(146, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.SwitchView();
            Window.Add(btn = new("Find...", new(152, 532), new(116, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.Find();
            Window.Add(btn = new("Go to...", new(272, 532), new(116, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.GoTo();


            //Header area
            Window.Add(txtBox = new("Header", 20, new(402, 2), new(96, 26), Highlights));
            Window.Add(lst = new(new(402, 32), new(396, 376))); lst.Id_Name = "HEADER_LIST"; FileHandler.SetHeader(lst); lst.CustomFont = Program.MonospaceFont; lst.UseCustomFont = true;
            Window.Add(btn = new("Change version", new(402, 412), new(176, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.ChangeVersion();
            Window.Add(btn = new("Change compression", new(582, 412), new(216, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.ChangeCompression();
            Window.Add(btn = new("Change field", new(402, 442), new(176, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.ChangeField();
            Window.Add(btn = new("Apply changes", new(582, 442), new(216, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.ApplyChanges();


            //Tools area
            Window.Add(txtBox = new("Tools", 20, new(402, 472), new(196, 26), Highlights));
            Window.Add(btn = new("Validate", new(402, 502), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.Validate();
            Window.Add(btn = new("Fix", new(502, 502), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.Fix();
            Window.Add(btn = new("Convert", new(402, 532), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.Convert();
            Window.Add(btn = new("Export", new(502, 532), new(96, 26))); btn.OnPressed += (object _, EventArgs _) => FileHandler.Export();
        }



        #region Button handles
        private static void OnFilesOpenPressed(object sender, EventArgs e)
        {
            PopupFilesOpen();
        }

        private static void OnPopupFilesOpenPressed(string status, string path)
        {
            if (status == "CANCEL") return;

            if (status == "INVALID")
            {
                Program.PopupErrorMessage("Invalid path/file!");
                return;
            }

            if (status != "OK") throw new NotImplementedException();

            if (!FileHandler.Open(path))
            {
                Program.PopupErrorMessage("Failed to open file!");
            }
        }
        #endregion



        #region Popups
        private static void PopupFilesOpen()
        {
            Button btn;
            Holder pop = new(new(275, 260));
            pop.Add(new Panel(new(0, 0), new(250, 80), new Color(100, 100, 100, 255)));
            pop.Add(new TextBox("Open:", new(10, 10), new(200, 20)));
            pop.Add(btn = new("Job", new(10, 50), new(100, 20))); btn.OnPressed += OnPopupFilesOpenOpenJobPressed;
            pop.Add(btn = new("Resource", new(140, 50), new(100, 20))); btn.OnPressed += OnPopupFilesOpenOpenResourcePressed;

            Program.ShowPopup(pop);
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

            Program.ShowPopup(pop);

            FileOpenUpdate(filter, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
        }




        private static void FileOpenUpdate(string filter, string path)
        {
            TextList lst = Program.GetOpenPopup().Children.First((IRenderable rend) => rend.Id_Name == "DIR_LISTING") as TextList;
            TextBox txt = Program.GetOpenPopup().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

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
        #endregion



        #region Popup button handles
        private static void OnPopupFileOpenPressed(Action<string, string> onClose)
        {
            TextList lst = Program.GetOpenPopup().Children.First((IRenderable rend) => rend.Id_Name == "DIR_LISTING") as TextList;
            TextBox txt = Program.GetOpenPopup().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

            try
            {
                string path = Path.Combine(txt.Text, lst.Lines[lst.Selected]);
                Program.ClosePopup();
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
            Program.ClosePopup();
        }
        private static void OnPopupFileSelectedPressed(string filter)
        {
            TextList lst = Program.GetOpenPopup().Children.First((IRenderable rend) => rend.Id_Name == "DIR_LISTING") as TextList;
            TextBox txt = Program.GetOpenPopup().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

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
            TextBox txt = Program.GetOpenPopup().Children.First((IRenderable rend) => rend.Id_Name == "CURRENT_PATH") as TextBox;

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
            Program.ClosePopup();
            PopupOpenFile("*.primejob", OnPopupFilesOpenPressed);
        }
        private static void OnPopupFilesOpenOpenResourcePressed(object sender, EventArgs e)
        {
            Program.ClosePopup();
            PopupOpenFile("*.rsrc", OnPopupFilesOpenPressed);
        }
        #endregion
    }
}
