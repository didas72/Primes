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

        private static void OnPopupFilesOpenPressed(FileSelector.OpenStatus status, string path)
        {
            Program.ClosePopup();

            if (status == FileSelector.OpenStatus.Cancel) return;

            if (status == FileSelector.OpenStatus.Invalid)
            {
                Program.PopupErrorMessage("Invalid path/file!");
                return;
            }

            if (status != FileSelector.OpenStatus.Ok) throw new NotImplementedException();

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
        private static void PopupOpenFile(string filter, Action<FileSelector.OpenStatus, string> onClose)
        {
            Holder pop = new(new(200, 175));
            pop.Add(new FileSelector(Vector2i.Zero, new(400, 250),
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                new string[] { filter, "*.*" }, onClose));

            Program.ShowPopup(pop);
        }
        #endregion



        #region Popup button handles
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
