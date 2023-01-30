using System;
using System.Collections.Generic;
using System.IO;

using DidasUtils.Numerics;

using Raylib_cs;

namespace Primes.UI.Render
{
    internal class FileSelector
    {
        private Holder internalHolder;
        private TextBox currentPathText;
        private TextList lineList;

        private string filter;


        public FileSelector(string homePath, string filter)
        {
            this.filter = filter;
            currentPathText.Text = homePath;

            Button btn;
            internalHolder = new(new(200, 175));
            internalHolder.Add(new Panel(new(0, 0), new(400, 250), Color.GRAY));
            internalHolder.Add(new TextBox("Select file to open:", new(10, 10), new(280, 20)));
            internalHolder.Add(currentPathText = new TextBox("", new(40, 30), new(350, 20)));
            internalHolder.Add(btn = new Button("^", new(10, 30), new(20, 20))); btn.OnPressed += (object sender, EventArgs e) => OnUpPressed();
            //internalHolder.Add(lineList = TextList.CreateSelectable(new(10, 50), new(380, 175))); lineList.OnSelected += (object sender, EventArgs e) => OnSelectedPressed(filter);
            //internalHolder.Add(btn = new Button("Cancel", new(180, 220), new(100, 20))); btn.OnPressed += (object sender, EventArgs e) => OnCancelPressed(onClose);
            //internalHolder.Add(btn = new Button("Open", new(290, 220), new(100, 20))); btn.OnPressed += (object sender, EventArgs e) => OnOpenPressed(onClose);

            Update();
        }



        public Holder GetHolder() => internalHolder;



        private void Update()
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
                string[] files = Directory.GetFiles(currentPathText.Text, filter);

                foreach (string dir in subDirs) //diff appearance for folders
                    lineList.Lines.Add(">" + Path.GetFileName(dir));
                foreach (string file in files)
                    lineList.Lines.Add(Path.GetFileName(file));
            }
        }

        private void OnUpPressed()
        {
            try
            {
                currentPathText.Text = Path.GetDirectoryName(currentPathText.Text);
                Update();
            }
            catch
            {
                currentPathText.Text = string.Empty;
                Update();
            }
        }
        private void OnCancelPressed()
        {

        }
    }
}
