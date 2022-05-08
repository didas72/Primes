using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;

using Microsoft.Win32;
using Microsoft.VisualBasic;

using DidasUtils;
using DidasUtils.Logging;

using Primes.Common;
using Primes.Common.Files;
using Primes.UI.Render;

namespace Primes.UI
{
    internal static class FileHandler
    {
        private static TextList content, header;



        public static void SetContent(TextList list) => content = list;
        public static void SetHeader(TextList list) => header = list;



        public static bool Open()
        {
            //TODO: Error, thread not STA :/
            //Custom file dialogs we go :(
            OpenFileDialog dialog = new()
            {
                Title = "Choose file",
                DefaultExt = "primejob",
                Filter = "PrimeJob files|*.primejob|Resource files|*.rsrc|All files|*.*",
                CheckFileExists = true,
                CheckPathExists = true,
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "primes"),
                Multiselect = false,
            };

            var result = dialog.ShowDialog();

            if (result != DialogResult.OK)
                return false;

            if (Path.GetExtension(dialog.FileName).ToLowerInvariant() == ".rsrc")
                return OpenResource(dialog.FileName);
            else
                return OpenJob(dialog.FileName);
        }
        public static bool SaveJob(string path)
        {
            throw new NotImplementedException();
        }
        public static bool SaveResource(string path)
        {
            throw new NotImplementedException();
        }



        public static void CreateNewJob()
        {
            throw new NotImplementedException();
        }
        public static void CreateNewResource()
        {
            throw new NotImplementedException();
        }



        public static void SwitchView()
        {
            throw new NotImplementedException();
        }



        private static bool OpenJob(string path)
        {
            try
            {
                PrimeJob job = PrimeJob.Deserialize(path);

                header.Lines.Clear();
                content.Lines.Clear();

                BuildHeaderText(job);
                BuildContentText(job);
            }
            catch { return false; }

            throw new NotImplementedException();
        }
        private static bool OpenResource(string path)
        {
            throw new NotImplementedException();
        }



        private static void BuildHeaderText(PrimeJob job)
        {
            header.Lines.Add($"Version: {job.FileVersion}");
            if (job.FileVersion >= new PrimeJob.Version(1, 2, 0))
                header.Lines.Add($"Compression: {job.FileCompression}");
            if (job.FileVersion >= new PrimeJob.Version(1, 1, 0))
                header.Lines.Add($"Batch: {job.Batch}");
            header.Lines.Add($"Start: {job.Start}");
            header.Lines.Add($"Count: {job.Count}");
            header.Lines.Add($"Progress: {job.Progress}");
        }
        private static void BuildContentText(PrimeJob job)
        {
            return;
            throw new NotImplementedException(); //TODO: 
        }
    }
}
