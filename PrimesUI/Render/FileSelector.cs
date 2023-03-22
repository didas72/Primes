using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;

using DidasUtils.Numerics;

using Primes.UI.Windows;

using Raylib_cs;

namespace Primes.UI.Render
{
    internal class FileSelector : IRenderable, IUpdatable
    {
        public string Id_Name { get; set; }
        public Vector2i Position { get; set; }


        private readonly Holder internalHolder;
        private readonly TextBox currentPathText;
        private readonly TextList lineList;
        private readonly Button filterButton;

        private readonly string[] filters;
        private int selectedFilter = 0;

        private readonly Action<OpenStatus, string> onClose;


        public FileSelector(Vector2i position, Vector2i size, string homePath, string[] filters, Action<OpenStatus, string> onClose)
        {
            if (filters == null || filters.Length == 0)
                throw new ArgumentException(string.Format("Filters cannot not be null and contain at least one filter."), nameof(filters));

            Position = position;
            this.filters = filters;
            this.onClose = onClose;

            Button btn;
            internalHolder = new(Vector2i.Zero);
            internalHolder.Add(new Panel(new(0, 0), size, BaseWindow.Background));
            internalHolder.Add(new TextBox("Select file to open:", new(10, 10), new(200, 20)));
            internalHolder.Add(filterButton = new Button(filters[0], 15, new(250, 10), new(140, 20))); filterButton.OnPressed += (object sender, EventArgs e) => OnFilterCycle();
            internalHolder.Add(currentPathText = new TextBox(homePath, new(40, 30), new(350, 20)));
            internalHolder.Add(btn = new Button("^", new(10, 30), new(20, 20))); btn.OnPressed += (object sender, EventArgs e) => OnUpPressed();
            internalHolder.Add(lineList = TextList.CreateSelectable(new(10, 50), new(380, 175))); lineList.OnSelected += (object sender, EventArgs e) => OnListSelected();
            internalHolder.Add(btn = new Button("Cancel", new(180, 220), new(100, 20))); btn.OnPressed += (object sender, EventArgs e) => OnCancelPressed();
            internalHolder.Add(btn = new Button("Open", new(290, 220), new(100, 20))); btn.OnPressed += (object sender, EventArgs e) => OnOpenPressed();

            UpdateContent();
        }



        public static FileSelector CreateGeneric(Vector2i position, Vector2i size, Action<OpenStatus, string> onClose)
            => new(position, size, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), new string[1] {"*.*"}, onClose);



        public Holder GetHolder() => internalHolder;



        public void Update(Vector2i localOffset)
        {
            internalHolder.Update(localOffset + Position);
        }
        public void Render(Vector2i localOffset)
        {
            internalHolder.Render(localOffset + Position);
        }


        private void UpdateContent()
        {
            lineList.Lines.Clear();

            if (currentPathText.Text == string.Empty) //drive list
            {
                foreach (string d in Environment.GetLogicalDrives())
                    lineList.Lines.Add(">" + d);
            }
            else
            {
                string[] subDirs = Directory.GetDirectories(currentPathText.Text);
                string[] files = Directory.GetFiles(currentPathText.Text, filters[selectedFilter]);

                foreach (string dir in subDirs) //diff appearance for folders
                    lineList.Lines.Add(">" + Path.GetFileName(dir));
                foreach (string file in files)
                    lineList.Lines.Add(Path.GetFileName(file));
            }
        }


        private void OnFilterCycle()
        {
            selectedFilter = (selectedFilter + 1) % filters.Length;
            filterButton.Text = filters[selectedFilter];
            UpdateContent();
        }
        private void OnUpPressed()
        {
            try
            {
                currentPathText.Text = Path.GetDirectoryName(currentPathText.Text);
                UpdateContent();
            }
            catch
            {
                currentPathText.Text = string.Empty;
                UpdateContent();
            }
        }
        private void OnListSelected()
        {
            string file = lineList.Lines[lineList.Selected];

            if (file.StartsWith('>')) //directory
            {
                currentPathText.Text = Path.Combine(currentPathText.Text, file.TrimStart('>'));
                UpdateContent();
            }

            //Ignore if selecting file
        }
        private void OnOpenPressed()
        {
            string path = Path.Combine(currentPathText.Text, lineList.Lines[lineList.Selected]);

            if (!File.Exists(path))
                onClose?.Invoke(OpenStatus.Invalid, string.Empty);
            else
                onClose?.Invoke(OpenStatus.Ok, path);
        }
        private void OnCancelPressed()
        {
            onClose?.Invoke(OpenStatus.Cancel, string.Empty);
        }



        public enum OpenStatus
        {
            None = -1,
            Ok,
            Cancel,
            Invalid
        }
    }
}
