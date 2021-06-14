using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace PrimesUI
{
    partial class PrimesUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TopPanel = new System.Windows.Forms.Panel();
            this.ExitButton = new System.Windows.Forms.Button();
            this.MyName = new System.Windows.Forms.Label();
            this.MenuPanel = new System.Windows.Forms.Panel();
            this.OptionsButton = new System.Windows.Forms.Button();
            this.TestingButton = new System.Windows.Forms.Button();
            this.FileViewerButton = new System.Windows.Forms.Button();
            this.FilesButton = new System.Windows.Forms.Button();
            this.ProgressButton = new System.Windows.Forms.Button();
            this.MainMenuButton = new System.Windows.Forms.Button();
            this.MainMenuPanel = new System.Windows.Forms.Panel();
            this.ProgressMenuPanel = new System.Windows.Forms.Panel();
            this.BatchProgress = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.FilesMenuPanel = new System.Windows.Forms.Panel();
            this.WorkerProgressList = new System.Windows.Forms.ListView();
            this.WorkerID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Batch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Progress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.MainResumeButton = new System.Windows.Forms.Button();
            this.TopPanel.SuspendLayout();
            this.MenuPanel.SuspendLayout();
            this.MainMenuPanel.SuspendLayout();
            this.ProgressMenuPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // TopPanel
            // 
            this.TopPanel.AccessibleName = "";
            this.TopPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.TopPanel.Controls.Add(this.ExitButton);
            this.TopPanel.Controls.Add(this.MyName);
            this.TopPanel.Location = new System.Drawing.Point(0, 0);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = new System.Drawing.Size(1050, 46);
            this.TopPanel.TabIndex = 0;
            // 
            // ExitButton
            // 
            this.ExitButton.AccessibleName = "ExitButton";
            this.ExitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ExitButton.FlatAppearance.BorderSize = 0;
            this.ExitButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.ExitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ExitButton.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.ExitButton.Location = new System.Drawing.Point(1003, 0);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(47, 46);
            this.ExitButton.TabIndex = 1;
            this.ExitButton.Text = "X";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // MyName
            // 
            this.MyName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.MyName.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.MyName.Location = new System.Drawing.Point(0, 0);
            this.MyName.Name = "MyName";
            this.MyName.Size = new System.Drawing.Size(583, 46);
            this.MyName.TabIndex = 0;
            this.MyName.Text = "PrimesUI by Didas72 and PeakRead";
            this.MyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MenuPanel
            // 
            this.MenuPanel.Controls.Add(this.OptionsButton);
            this.MenuPanel.Controls.Add(this.TestingButton);
            this.MenuPanel.Controls.Add(this.FileViewerButton);
            this.MenuPanel.Controls.Add(this.FilesButton);
            this.MenuPanel.Controls.Add(this.ProgressButton);
            this.MenuPanel.Controls.Add(this.MainMenuButton);
            this.MenuPanel.Location = new System.Drawing.Point(0, 46);
            this.MenuPanel.Name = "MenuPanel";
            this.MenuPanel.Size = new System.Drawing.Size(175, 531);
            this.MenuPanel.TabIndex = 1;
            // 
            // OptionsButton
            // 
            this.OptionsButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.OptionsButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.OptionsButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.OptionsButton.FlatAppearance.BorderSize = 0;
            this.OptionsButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.OptionsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.OptionsButton.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.OptionsButton.Location = new System.Drawing.Point(0, 231);
            this.OptionsButton.Name = "OptionsButton";
            this.OptionsButton.Size = new System.Drawing.Size(175, 46);
            this.OptionsButton.TabIndex = 5;
            this.OptionsButton.Text = "Options";
            this.OptionsButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.OptionsButton.UseVisualStyleBackColor = false;
            // 
            // TestingButton
            // 
            this.TestingButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.TestingButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.TestingButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.TestingButton.FlatAppearance.BorderSize = 0;
            this.TestingButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.TestingButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TestingButton.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TestingButton.Location = new System.Drawing.Point(0, 185);
            this.TestingButton.Name = "TestingButton";
            this.TestingButton.Size = new System.Drawing.Size(175, 46);
            this.TestingButton.TabIndex = 4;
            this.TestingButton.Text = "Testing";
            this.TestingButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.TestingButton.UseVisualStyleBackColor = false;
            // 
            // FileViewerButton
            // 
            this.FileViewerButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.FileViewerButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.FileViewerButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.FileViewerButton.FlatAppearance.BorderSize = 0;
            this.FileViewerButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.FileViewerButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FileViewerButton.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.FileViewerButton.Location = new System.Drawing.Point(0, 138);
            this.FileViewerButton.Name = "FileViewerButton";
            this.FileViewerButton.Size = new System.Drawing.Size(175, 46);
            this.FileViewerButton.TabIndex = 3;
            this.FileViewerButton.Text = "File Viewer";
            this.FileViewerButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.FileViewerButton.UseVisualStyleBackColor = false;
            // 
            // FilesButton
            // 
            this.FilesButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.FilesButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.FilesButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.FilesButton.FlatAppearance.BorderSize = 0;
            this.FilesButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.FilesButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FilesButton.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.FilesButton.Location = new System.Drawing.Point(0, 92);
            this.FilesButton.Name = "FilesButton";
            this.FilesButton.Size = new System.Drawing.Size(175, 46);
            this.FilesButton.TabIndex = 2;
            this.FilesButton.Text = "Files";
            this.FilesButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.FilesButton.UseVisualStyleBackColor = false;
            this.FilesButton.Click += new System.EventHandler(this.FilesMenuButton_Click);
            // 
            // ProgressButton
            // 
            this.ProgressButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.ProgressButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ProgressButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.ProgressButton.FlatAppearance.BorderSize = 0;
            this.ProgressButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.ProgressButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ProgressButton.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.ProgressButton.Location = new System.Drawing.Point(0, 46);
            this.ProgressButton.Name = "ProgressButton";
            this.ProgressButton.Size = new System.Drawing.Size(175, 46);
            this.ProgressButton.TabIndex = 1;
            this.ProgressButton.Text = "Progress";
            this.ProgressButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ProgressButton.UseVisualStyleBackColor = false;
            this.ProgressButton.Click += new System.EventHandler(this.ProgressMenuButton_Click);
            // 
            // MainMenuButton
            // 
            this.MainMenuButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.MainMenuButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.MainMenuButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.MainMenuButton.FlatAppearance.BorderSize = 0;
            this.MainMenuButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.MainMenuButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MainMenuButton.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.MainMenuButton.Location = new System.Drawing.Point(0, 0);
            this.MainMenuButton.Name = "MainMenuButton";
            this.MainMenuButton.Size = new System.Drawing.Size(175, 46);
            this.MainMenuButton.TabIndex = 0;
            this.MainMenuButton.Text = "Main Menu";
            this.MainMenuButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.MainMenuButton.UseVisualStyleBackColor = false;
            this.MainMenuButton.Click += new System.EventHandler(this.MainMenuButton_Click);
            // 
            // MainMenuPanel
            // 
            this.MainMenuPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.MainMenuPanel.Controls.Add(this.ProgressMenuPanel);
            this.MainMenuPanel.Controls.Add(this.WorkerProgressList);
            this.MainMenuPanel.Controls.Add(this.button1);
            this.MainMenuPanel.Controls.Add(this.MainResumeButton);
            this.MainMenuPanel.Location = new System.Drawing.Point(175, 46);
            this.MainMenuPanel.Name = "MainMenuPanel";
            this.MainMenuPanel.Size = new System.Drawing.Size(875, 531);
            this.MainMenuPanel.TabIndex = 2;
            // 
            // ProgressMenuPanel
            // 
            this.ProgressMenuPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.ProgressMenuPanel.Controls.Add(this.FilesMenuPanel);
            this.ProgressMenuPanel.Controls.Add(this.BatchProgress);
            this.ProgressMenuPanel.Controls.Add(this.button2);
            this.ProgressMenuPanel.Controls.Add(this.button3);
            this.ProgressMenuPanel.Location = new System.Drawing.Point(0, 0);
            this.ProgressMenuPanel.Name = "ProgressMenuPanel";
            this.ProgressMenuPanel.Size = new System.Drawing.Size(875, 531);
            this.ProgressMenuPanel.TabIndex = 4;
            this.ProgressMenuPanel.Visible = false;
            // 
            // BatchProgress
            // 
            this.BatchProgress.Font = new System.Drawing.Font("Arial", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.BatchProgress.Location = new System.Drawing.Point(0, 46);
            this.BatchProgress.Name = "BatchProgress";
            this.BatchProgress.Size = new System.Drawing.Size(200, 35);
            this.BatchProgress.TabIndex = 4;
            this.BatchProgress.Text = "Batch progress: XXX.X%";
            this.BatchProgress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button2
            // 
            this.button2.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.button2.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(117, 0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(117, 35);
            this.button2.TabIndex = 3;
            this.button2.Text = "Pause";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.button3.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.Location = new System.Drawing.Point(0, 0);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(117, 35);
            this.button3.TabIndex = 2;
            this.button3.Text = "Resume";
            this.button3.UseVisualStyleBackColor = true;
            // 
            // FilesMenuPanel
            // 
            this.FilesMenuPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.FilesMenuPanel.Location = new System.Drawing.Point(0, 0);
            this.FilesMenuPanel.Name = "FilesMenuPanel";
            this.FilesMenuPanel.Size = new System.Drawing.Size(875, 531);
            this.FilesMenuPanel.TabIndex = 5;
            this.FilesMenuPanel.Visible = false;
            // 
            // WorkerProgressList
            // 
            this.WorkerProgressList.AutoArrange = false;
            this.WorkerProgressList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.WorkerProgressList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.WorkerProgressList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.WorkerID,
            this.Batch,
            this.Progress});
            this.WorkerProgressList.Enabled = false;
            this.WorkerProgressList.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.WorkerProgressList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.WorkerProgressList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.WorkerProgressList.HideSelection = false;
            this.WorkerProgressList.LabelWrap = false;
            this.WorkerProgressList.Location = new System.Drawing.Point(0, 46);
            this.WorkerProgressList.Margin = new System.Windows.Forms.Padding(0);
            this.WorkerProgressList.MultiSelect = false;
            this.WorkerProgressList.Name = "WorkerProgressList";
            this.WorkerProgressList.Scrollable = false;
            this.WorkerProgressList.ShowGroups = false;
            this.WorkerProgressList.Size = new System.Drawing.Size(875, 485);
            this.WorkerProgressList.TabIndex = 3;
            this.WorkerProgressList.UseCompatibleStateImageBehavior = false;
            this.WorkerProgressList.View = System.Windows.Forms.View.Details;
            // 
            // WorkerID
            // 
            this.WorkerID.Text = "Worker ID";
            this.WorkerID.Width = 80;
            // 
            // Batch
            // 
            this.Batch.Text = "Batch";
            this.Batch.Width = 80;
            // 
            // Progress
            // 
            this.Progress.Text = "Progress";
            this.Progress.Width = 500;
            // 
            // button1
            // 
            this.button1.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.button1.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(117, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(117, 35);
            this.button1.TabIndex = 1;
            this.button1.Text = "Pause";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.PauseButton_Click);
            // 
            // MainResumeButton
            // 
            this.MainResumeButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.MainResumeButton.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(80)))));
            this.MainResumeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.MainResumeButton.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainResumeButton.Location = new System.Drawing.Point(0, 0);
            this.MainResumeButton.Name = "MainResumeButton";
            this.MainResumeButton.Size = new System.Drawing.Size(117, 35);
            this.MainResumeButton.TabIndex = 0;
            this.MainResumeButton.Text = "Resume";
            this.MainResumeButton.UseVisualStyleBackColor = true;
            this.MainResumeButton.Click += new System.EventHandler(this.ResumeButton_Click);
            // 
            // PrimesUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.ClientSize = new System.Drawing.Size(1050, 577);
            this.Controls.Add(this.MainMenuPanel);
            this.Controls.Add(this.MenuPanel);
            this.Controls.Add(this.TopPanel);
            this.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.Silver;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PrimesUI";
            this.ShowIcon = false;
            this.Text = "Form1";
            this.TopPanel.ResumeLayout(false);
            this.MenuPanel.ResumeLayout(false);
            this.MainMenuPanel.ResumeLayout(false);
            this.ProgressMenuPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel TopPanel;
        private Label MyName;
        private Panel MenuPanel;
        private Button MainMenuButton;
        private Button ExitButton;
        private Button OptionsButton;
        private Button TestingButton;
        private Button FileViewerButton;
        private Button FilesButton;
        private Button ProgressButton;
        private Panel MainMenuPanel;
        private Button button1;
        private Button MainResumeButton;
        private ListView WorkerProgressList;
        private ColumnHeader WorkerID;
        private ColumnHeader Batch;
        private ColumnHeader Progress;
        private Panel ProgressMenuPanel;
        private Label BatchProgress;
        private Button button2;
        private Button button3;
        private Panel FilesMenuPanel;


        #region UI Handlers
        private void ExitButton_Click(object sender, EventArgs e) => PrimesProgram.Exit();

        private void MainMenuButton_Click(object sender, EventArgs e)
        {
            MainMenuPanel.Visible = true;
            ProgressMenuPanel.Visible = false;
            FilesMenuPanel.Visible = false;
        }

        private void ProgressMenuButton_Click(object sender, EventArgs e)
        {
            MainMenuPanel.Visible = true;
            ProgressMenuPanel.Visible = true;
            FilesMenuPanel.Visible = false;
        }

        private void FilesMenuButton_Click(object sender, EventArgs e)
        {
            MainMenuPanel.Visible = true;
            ProgressMenuPanel.Visible = true;
            FilesMenuPanel.Visible = true;
        }

        private void ResumeButton_Click(object sender, EventArgs e) => PrimesProgram.Resume();

        private void PauseButton_Click(object sender, EventArgs e) => PrimesProgram.Pause();

        private void AddNewListElement(int index, int batch)
        {
            ListViewItem item = new ListViewItem();

            item.Text = $"Worker #{index}";
            item.SubItems.Add(batch.ToString());
            WorkerProgressList.Controls.Add(new ProgressBar() { Value = 50, Height = 15, Width = 250, Left = 160, Top = 5 + index * 20 });

            WorkerProgressList.Items.Add(item);
        }
        #endregion
    }
}
