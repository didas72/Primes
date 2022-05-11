﻿using System;
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
            throw new NotImplementedException();
        }
        public static bool SaveResource(string path)
        {
            throw new NotImplementedException();
        }



        public static void CreateNewJob()
        {
            currentJob = new PrimeJob(PrimeJob.Version.Latest, PrimeJob.Comp.Default, 0, 0, 0, 0, new());
            currentViewResource = false;
            BuildTexts();
            //TODO: Anything else?
        }
        public static void CreateNewResource()
        {
            currentResource = new KnownPrimesResourceFile(KnownPrimesResourceFile.Version.Latest, KnownPrimesResourceFile.Comp.Default, Array.Empty<ulong>());
            currentViewResource = true;
            throw new NotImplementedException(); //TODO: Build texts for these
        }



        public static void SwitchView()
        {
            BuildContentText(!currentViewBinary);
        }
        public static void Find()
        {
            throw new NotImplementedException();
        }



        public static void ChangeVersion()
        {
            throw new NotImplementedException();
        }
        public static void ChangeCompression()
        {
            throw new NotImplementedException();
        }
        public static void ChangeField()
        {
            throw new NotImplementedException();
        }
        public static void ApplyChanges()
        {
            throw new NotImplementedException();
        }



        public static void Validate()
        {
            throw new NotImplementedException();
        }
        public static void Fix()
        {
            throw new NotImplementedException();
        }
        public static void Convert()
        {
            throw new NotImplementedException();
        }
        public static void Export()
        {
            throw new NotImplementedException();
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
            currentViewResource = true;
            throw new NotImplementedException();
        }



        private static void BuildTexts()
        {
            BuildHeaderText();
            BuildContentText(false);
        }
        private static void BuildHeaderText()
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
        private static void BuildContentText(bool binaryView)
        {
            currentViewBinary = binaryView;

            content.Lines.Clear();

            if (binaryView)
                BuildBinaryContentText();
            else
                BuildNormalContentText();
        }
        private static void BuildBinaryContentText()
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
        private static void BuildNormalContentText()
        {
            for (int i = 0; i < currentJob.Primes.Count; i++)
            {
                content.Lines.Add($"{i:D4}: {currentJob.Primes[i]}");
            }
        }
    }
}