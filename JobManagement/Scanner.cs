using System;
using System.IO;
using System.Threading;

using Primes.Common;
using Primes.Common.Files;

namespace JobManagement
{
    public class Scanner
    {
        private readonly string SCAN_LOCK = "";

        private ScanResults results;
        private string sourceDirectory, tmpDirectory, destinationDirectory, rejectsDirectory;

        private bool batchNeedsRepacking = false;
        private bool batchNeedsRejecting = false;

        public TimeSpan lastScanTime = new TimeSpan();
        public volatile int currentBatch = 0, batchCount = 0;



        public ScanResults RunScan(string sourceDirectory, string tmpDirectory, string destinationDirectory, string rejectsDirectory)
        {
            lock (SCAN_LOCK)
            {
                this.sourceDirectory = sourceDirectory;
                this.tmpDirectory = tmpDirectory;
                this.destinationDirectory = destinationDirectory;
                this.rejectsDirectory = rejectsDirectory;

                RunAllScans();
            }

            return results;
        }



        private void RunAllScans()
        {
            results = new ScanResults();

            //clear and create dirs if needed
            Log.LogEvent("Creating directories...", "Scanner");
            try { Utils.DeleteDirectory(tmpDirectory); } catch { }
            try { Utils.DeleteDirectory(destinationDirectory); } catch { }
            try { Utils.DeleteDirectory(rejectsDirectory); } catch { }
            Directory.CreateDirectory(tmpDirectory);
            Directory.CreateDirectory(destinationDirectory);
            Directory.CreateDirectory(rejectsDirectory);
            Log.LogEvent("Directories created.", "Scanner");

            Thread.Sleep(1000);



            Log.LogEvent("Retrieving batch list...", "Scanner");
            string[] batches = Directory.GetFiles(sourceDirectory, "*.7z");
            batchCount = batches.Length;
            Log.LogEvent("Batch list retrieved.", "Scanner");



            Log.LogEvent("Starting scan...", "Scanner");

            for (int b = 0; b < batches.Length; b++)
            {
                DateTime start = DateTime.Now;
                Program.Print($"Scanning batch {Path.GetFileName(batches[b])}");

                batchNeedsRepacking = false;
                batchNeedsRejecting = false;

                string batchSourcePath = batches[b];
                string batchFileName = Path.GetFileNameWithoutExtension(batchSourcePath);
                string batchFinalPath = Path.Combine(destinationDirectory, batchFileName + ".7z");


                DecompressAndScanBatch(batchFileName, batchSourcePath, out string batchUncompressPath);


                Log.LogEvent("Retrieveing job list...", "Scanner");
                string[] jobs = Directory.GetFiles(batchUncompressPath, "*.primejob");
                Log.LogEvent("Job list retrieved.", "Scanner");



                Log.LogEvent("Scanning jobs...", "Scanner");

                for (int j = 0; j < jobs.Length; j++)
                {
                    string jobSourcePath = jobs[j];

                    LoadAndScanJob(jobSourcePath);
                }

                Log.LogEvent("Jobs scanned.", "Scanner");

                if (batchNeedsRepacking)
                {
                    Log.LogEvent($"Repacking batch {batchFileName}.", "Scanner");

                    if (!SevenZip.TryCompress7z(batchUncompressPath, batchFinalPath))
                    {
                        Log.LogEvent(Log.EventType.Error, $"Failed to repack batch {batchFileName}.", "Scanner");
                    }
                }
                else if (batchNeedsRejecting)
                {
                    Log.LogEvent(Log.EventType.Warning, $"Rejecting batch {batchFileName}.", "Scanner");

                    if (!SevenZip.TryCompress7z(batchUncompressPath, Path.Combine(rejectsDirectory, batchFileName + ".rejected.7z")))
                    {
                        Log.LogEvent(Log.EventType.Error, $"Failed to repack batch {batchFileName}.", "Scanner");
                    }
                }
                else
                {
                    Log.LogEvent($"Copying batch {batchFileName} to final destination.", "Scanner");
                    File.Copy(batchSourcePath, batchFinalPath);
                }

                lastScanTime = DateTime.Now - start;
                currentBatch = b + 1;
            }

            Log.LogEvent("Scan complete.", "Scanner");
        }



        private bool DecompressAndScanBatch(string batchFileName, string batchSourcePath, out string batchUncompressPath)
        {
            Log.LogEvent($"Decompressing and scanning batch {batchFileName}.", "ScannerBatchScan");

            string batchCopyPath = Path.Combine(tmpDirectory, batchFileName + ".7z");
            batchUncompressPath = tmpDirectory;

            //copy file to ensure no interaction with source files
            File.Copy(batchSourcePath, batchCopyPath);

            //try to uncompress and deal with it if failed
            bool uncompressSuccess = SevenZip.TryDecompress7z(batchCopyPath, batchUncompressPath);
            if (!uncompressSuccess)
            {
                Log.LogEvent(Log.EventType.Error, $"Failed to decompress batch {batchFileName} with seven zip. Skipping", "ScannerBatchScan");
                lock (Program.prints)
                {
                    Program.prints.Add($"Failed to decompress batch {batchFileName} with seven zip. Skipping");
                }

                return false;
            }

            //register stats
            results.ZippedSizes.Add(new FileInfo(batchSourcePath).Length);

            return true;
        }
        private bool LoadAndScanJob(string jobPath)
        {
            Log.LogEvent($"Decompressing and scanning job {Path.GetFileName(jobPath)}.", "ScannerJobScan");

            PrimeJob job = PrimeJob.Deserialize(jobPath);

            if (!PrimeJob.CheckJob(job, true, out string msg1))
            {
                Log.LogEvent(Log.EventType.Warning, $"Primejob {Path.GetFileName(jobPath)} failed basic integrity tests. Correcting duplicates and rescanning. Message: {msg1}", "ScannerJobScan");
                Program.Print($"Primejob {Path.GetFileName(jobPath)} failed basic integrity tests. Correcting duplicates and rescanning.");

                //check again, if errors presist it is not just a matter of duplicates and needs manual fixing/replacing
                if (!PrimeJob.CheckJob(job, false, out string msg2))
                {
                    Log.LogEvent(Log.EventType.HighWarning, $"Primejob {Path.GetFileName(jobPath)} failed basic integrity tests a second time. Moving to rejects. Message: {msg2}", "ScannerJobScan");
                    Program.Print($"Primejob {Path.GetFileName(jobPath)} failed basic integrity tests a second time. Moving to rejects.");

                    File.Move(jobPath, Path.Combine(rejectsDirectory, Path.GetFileName(jobPath) + ".rejected"));
                    batchNeedsRejecting = true;

                    return false;
                }

                Log.LogEvent($"Primejob {Path.GetFileName(jobPath)} sucessfully corrected. Updating file.", "ScannerJobScan");
                Program.Print($"Primejob {Path.GetFileName(jobPath)} sucessfully corrected. Updating file.");

                //save if correction was successful
                PrimeJob.Serialize(job, jobPath);

                batchNeedsRepacking = true;
            }

            //register stats
            results.NCCSizes.Add(new FileInfo(jobPath).Length);
            results.RawSizes.Add(PrimeJob.RawFileSize(job));
            results.PrimesPerFiles.Add(job.Primes.Count);
            results.PrimeDensities.Add(new PrimeDensity(job.Start, job.Count, (ulong)job.Primes.Count));

            //scan for twin primes
            ulong last = job.Primes[0];
            for (int i = 1; i < job.Primes.Count; i++)
            {
                if (job.Primes[i] - last <= 2)
                    results.TwinPrimes.Add(new TwinPrimes(last, job.Primes[i]));
            }

            return true;
        }
    }
}
