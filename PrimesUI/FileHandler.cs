using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Globalization;
//using System.Windows.Forms;

using Microsoft.Win32;
using Microsoft.VisualBasic;

using Raylib_cs;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Numerics;

using Primes.Common;
using Primes.Common.Files;
using Primes.UI.Render;

namespace Primes.UI
{
    internal static class FileHandler
    {
        private static TextList content, header;
        private static bool currentViewBinary = false;
        private static bool currentViewResource = false;

        private static PrimeJob currentJob;
        private static KnownPrimesResourceFile currentResource;



        public static void SetContent(TextList list) => content = list;
        public static void SetHeader(TextList list) => header = list;



        public static bool Open(string path)
        {
            if (Path.GetExtension(path).ToLowerInvariant() == ".rsrc")
                return OpenResource(path);
            else
                return OpenJob(path);
        }
        public static bool SaveJob(string path)
        {
            throw new NotImplementedException(); //TODO: Implement save job
        }
        public static bool SaveResource(string path)
        {
            throw new NotImplementedException();  //TODO: Implement save resource
        }



        public static void CreateNewJob()
        {
            currentJob = new PrimeJob(PrimeJob.Version.Latest, PrimeJob.Comp.Default, 0, 0, 0, 0, new());
            currentViewResource = false;
            BuildTexts();
        }
        public static void CreateNewResource()
        {
            currentResource = new KnownPrimesResourceFile(KnownPrimesResourceFile.Version.Latest, KnownPrimesResourceFile.Comp.Default, Array.Empty<ulong>());
            currentViewResource = true;
            throw new NotImplementedException(); //TODO: Limit resource loading and text building
        }



        public static void SwitchView()
        {
            if (currentJob == null && currentResource == null)
            { Program.PopupErrorMessage("No file open."); return; }

            BuildContentText(!currentViewBinary);
        }
        public static void Find()
        {
            if (currentJob == null && currentResource == null)
            { Program.PopupErrorMessage("No file open."); return; }

            Button btn; InputField inp;
            Holder pop = new(new(250, 230));
            pop.Add(new Panel(new(0, 0), new(300, 120), new Color(50, 50, 50, 255)));
            pop.Add(new TextBox("Number to find:", new(10, 10), new(280, 20)));
            pop.Add(inp = new(new(10, 35), new(280, 20)));
            pop.Add(btn = new("HEX", new(10, 85), new(70, 20))); btn.OnPressed += (object sender, EventArgs _) => OnFindSelected(inp, true);
            pop.Add(btn = new("DEC", new(90, 85), new(70, 20))); btn.OnPressed += (object sender, EventArgs _) => OnFindSelected(inp, false);
            pop.Add(btn = new("Cancel", new(210, 85), new(80, 20))); btn.OnPressed += (object sender, EventArgs _) => Program.ClosePopup();

            Program.ShowPopup(pop);
        }
        private static void OnFindSelected(InputField inp, bool hex)
        {
            if (!ulong.TryParse(inp.Text, hex ? NumberStyles.HexNumber : NumberStyles.Integer, null, out ulong search))
            {
                Program.PopupErrorMessage("Provided index was in bad format.");
                return;
            }

            ulong[] pr = currentViewResource ? currentResource.Primes : currentJob.Primes.ToArray();

            int min = 0, max = pr.Length / 2, index;

            while (true)
            {
                index = (min + max) / 2;

                if (pr[index] < search)
                    min = index;
                else if (pr[index] > search)
                    max = index; 
                else break;

                if (max - min <= 1) break;
            }

            content.Selected = index / (currentViewBinary ? 8 : 1);
            content.Scroll = content.Selected;

            Program.ClosePopup();
        }
        public static void GoTo()
        {
            if (currentJob == null && currentResource == null)
            { Program.PopupErrorMessage("No file open."); return; }

            Button btn; InputField inp;
            Holder pop = new(new(250, 230));
            pop.Add(new Panel(new(0, 0), new(300, 120), new Color(50, 50, 50, 255)));
            pop.Add(new TextBox("Index to go to:", new(10, 10), new(280, 20)));
            pop.Add(inp = new(new(10, 35), new(280, 20)));
            pop.Add(btn = new("HEX", new(10, 85), new(70, 20))); btn.OnPressed += (object sender, EventArgs _) => OnGotoSelected(inp, true);
            pop.Add(btn = new("DEC", new(90, 85), new(70, 20))); btn.OnPressed += (object sender, EventArgs _) => OnGotoSelected(inp, false);
            pop.Add(btn = new("Cancel", new(210, 85), new(80, 20))); btn.OnPressed += (object sender, EventArgs _) => Program.ClosePopup();

            Program.ShowPopup(pop);
        }
        private static void OnGotoSelected(InputField inp, bool hex)
        {
            if (!int.TryParse(inp.Text, hex ? NumberStyles.HexNumber : NumberStyles.Integer, null, out int index))
            {
                Program.PopupErrorMessage("Provided index was in bad format.");
                return;
            }

            content.Selected = index / (currentViewBinary ? 8 : 1);
            content.Scroll = content.Selected;

            Program.ClosePopup();
        }



        public static void ChangeVersion()
        {
            throw new NotImplementedException(); //TODO: Implement change version
        }
        public static void ChangeCompression()
        {
            throw new NotImplementedException(); //TODO: Implement change compressiong
        }
        public static void ChangeField()
        {
            throw new NotImplementedException(); //TODO: Implement change field
        }
        public static void ApplyChanges()
        {
            throw new NotImplementedException(); //TODO: Implement apply changes
        }



        public static void Validate()
        {
            if (currentViewResource)
            {
                Program.PopupErrorMessage("Cannot validate resources.");
                return;
            }
            if (currentJob == null)
            {
                Program.PopupErrorMessage("No job is currently loaded.");
                return;
            }

            if (PrimeJob.CheckJob(currentJob, false, out string msg))
            {
                Program.PopupStatusMessage("Job validation", "Successfully validated job.");
            }
            else
            {
                Program.PopupStatusMessage("Job validation", $"Validation failed: {msg}");
            }
        }
        public static void Fix()
        {
            throw new NotImplementedException();  //TODO: Implement fix
        }
        public static void Convert()
        {
            throw new NotImplementedException();  //TODO: Implement convert
        }
        public static void Export()
        {
            if (currentViewResource)
            {
                Program.PopupErrorMessage("Cannot export resources.");
                return;
            }
            if (currentJob == null)
            {
                Program.PopupErrorMessage("No job is currently loaded.");
                return;
            }

            Program.PopupChoiceMessage("Export", "CSV", "Text (lines)", OnExportChoice);

            //TODO:
            throw new NotImplementedException();
        }
        private static void OnExportChoice(object sender, int opt)
        {


            if (opt == 1)
                return; //ExportCSV(); //TODO: Save file popup (callback should call ExportCSV with proper arguments)
            else if (opt == 2)
                return; //ExportTxt(); //TODO: Save file popup (callback should call ExportTxt with proper arguments)
        }



        private static bool OpenJob(string path)
        {
            try
            {
                currentJob = PrimeJob.Deserialize(path);
                currentViewResource = false;
                BuildTexts();
            }
            catch (Exception e) { Log.LogException("Failed to open file!", "FileHandler", e); return false; }

            return true;
        }
        private static bool OpenResource(string path)
        {
            try
            {
                FileStream fs = File.OpenRead(path);
                currentResource = KnownPrimesResourceFile.Deserialize(fs);
                currentViewResource = true;
                fs.Close();
                BuildTexts();
            }
            catch (Exception e) { Log.LogException("Failed to open file!", "FileHandler", e); return false; }

            return true;
        }


        
        private static void ExportCSV(string path)
        {
            FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
            StreamWriter sw = new(fs);

            for (int i = 0; i < currentJob.Primes.Count; i++)
            {
                sw.WriteLine($"{i},{currentJob.Primes[i]}");
            }

            sw.Flush();
            sw.Close();
        }
        private static void ExportTxt(string path)
        {
            FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
            StreamWriter sw = new(fs);

            for (int i = 0; i < currentJob.Primes.Count; i++)
            {
                sw.WriteLine(currentJob.Primes[i].ToString());
            }

            sw.Flush();
            sw.Close();
        }



        #region TextBuilding
        private static void BuildTexts()
        {
            if (currentViewResource)
            {
                BuildResourceHeaderText();
                BuildResourceContentText(false);
            }
            else
            {
                BuildJobHeaderText();
                BuildJobContentText(false);
            }
        }

        private static void BuildContentText(bool binaryView)
        {
            if (currentViewResource)
                BuildResourceContentText(binaryView);
            else
                BuildJobContentText(binaryView);
        }

        private static void BuildJobHeaderText()
        {
            header.Lines.Clear();

            header.Lines.Add($"Version: {currentJob.FileVersion}");

            //version dependant
            if (currentJob.FileVersion >= new PrimeJob.Version(1, 1, 0))
                header.Lines.Add($"Batch: {currentJob.Batch}");
            if (currentJob.FileVersion >= new PrimeJob.Version(1, 2, 0))
                header.Lines.Add($"Compression: {currentJob.FileCompression}");
            
            header.Lines.Add($"Start: {currentJob.Start} ({Utils.FormatNumber(currentJob.Start).Replace(".00", string.Empty)})");
            header.Lines.Add($"Count: {currentJob.Count} ({Utils.FormatNumber(currentJob.Count).Replace(".00", string.Empty)})");
            header.Lines.Add($"Progress: {currentJob.Progress} ({(currentJob.Progress * 100f / (float)currentJob.Count).ToString("F2").Replace(".00", string.Empty)}%)");
        }
        private static void BuildJobContentText(bool binaryView)
        {
            currentViewBinary = binaryView;

            content.Lines.Clear();

            if (binaryView)
                BuildJobBinaryContentText();
            else
                BuildJobNormalContentText();
        }
        private static void BuildJobBinaryContentText()
        {
            MemoryStream ms = new();
            PrimeJob.Serialize(currentJob, ms);
            ms.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[8]; int len; string line;
            int lHead = 0;

            do
            {
                len = ms.Read(buffer, 0, buffer.Length);
                if (len == 0) break;

                line = $"0x{lHead:X6}: ";

                for (int i = 0; i < len; i++)
                {
                    line += $"{buffer[i]:X2} "; 
                }

                content.Lines.Add(line);

                lHead += 8;
            }
            while (len == buffer.Length);
        }
        private static void BuildJobNormalContentText()
        {
            for (int i = 0; i < currentJob.Primes.Count; i++)
            {
                content.Lines.Add($"{i:D4}: {currentJob.Primes[i]}");
            }
        }

        private static void BuildResourceHeaderText()
        {
            header.Lines.Clear();

            header.Lines.Add($"Version: {currentResource.FileVersion}");
            header.Lines.Add($"Primes count: {currentResource.Primes.Length}");

            if (currentResource.FileVersion.Equals(new KnownPrimesResourceFile.Version(1, 1, 0)))
                header.Lines.Add("Highest checked in file: (N/A)");
            else if (currentResource.FileVersion.Equals(new KnownPrimesResourceFile.Version(1, 2, 0)))
                header.Lines.Add($"Compression: {currentResource.FileCompression}");
        }
        private static void BuildResourceContentText(bool binaryView)
        {
            currentViewBinary = binaryView;

            content.Lines.Clear();

            if (binaryView)
                BuildResourceBinaryContentText();
            else
                BuildResourceNormalContentText();
        }
        private static void BuildResourceBinaryContentText()
        {
            currentViewBinary = false;

            throw new Exception("I guarantee you don't have RAM for this.");
        }
        private static void BuildResourceNormalContentText()
        {
            for (int i = 0; i < currentResource.Primes.Length; i++)
            {
                content.Lines.Add($"{i:D4}: {currentResource.Primes[i]}");
            }
        }
        #endregion
    }
}
