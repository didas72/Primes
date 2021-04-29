using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Primes.Common.Files
{
    public class KnownPrimesResourceFile
    {
        public readonly Version version;
        public ulong highestCheckedInFile;
        public ulong[] primes;



        public static KnownPrimesResourceFile empty = new KnownPrimesResourceFile(Version.zero, 0, new ulong[0]);



        public KnownPrimesResourceFile(Version version, ulong[] primes)
        {
            this.version = version; if (primes.Length > 0) highestCheckedInFile = primes.Last(); else highestCheckedInFile = 0; this.primes = primes;
        }
        public KnownPrimesResourceFile(Version version, ulong highestCheckedInFile, ulong[] primes)
        {
            this.version = version; this.highestCheckedInFile = highestCheckedInFile; this.primes = primes;
        }



        public static void Deserialize(string path, out KnownPrimesResourceFile file)
        {
            file = new KnownPrimesResourceFile(Version.zero, new ulong[] { 0 });

            /*  knownPrimes.rsrc v1.0.0
             *  3 bytes     Version     version (1 byte major, 1 byte minor, 1 byte patch)
             *  4 bytes     int         primesInFile
             *  xxx         ulong[]     primes
             */
            /*  knownPrimes.rsrc v1.1.0
             *  3 bytes     Version     version (1 byte major, 1 byte minor, 1 byte patch)
             *  8 bytes     ulong       highestCheckedInFile (highest number till which we checked to make this file, used to later append more primes to the file)
             *  4 bytes     int         primesInFile
             *  xxx         ulong[]     primes
             */

            byte[] bytes = File.ReadAllBytes(path);

            Version ver = new Version(bytes[0], bytes[1], bytes[2]);

            if (!ver.IsCompatible())
            {
                throw new IncompatibleVersionException($"Attempted to deserialize known primes resource of version {ver} but no serialization method was implemented for such version.");
            }
            else if (!ver.IsLatest())
            {
                if (ver.IsEqual(new Version(1, 0, 0)))
                {
                    int primesInFile = BitConverter.ToInt32(bytes, 3);

                    ulong[] primes = new ulong[primesInFile];
                    Buffer.BlockCopy(bytes, 7, primes, 0, primesInFile * 8);

                    file = new KnownPrimesResourceFile(ver, primes);
                }
            }
            else //it is latest
            {
                ulong highestCheckedInFile = BitConverter.ToUInt64(bytes, 3);
                int primesInFile = BitConverter.ToInt32(bytes, 11);

                ulong[] primes = new ulong[primesInFile];
                Buffer.BlockCopy(bytes, 15, primes, 0, primesInFile * 8);

                file = new KnownPrimesResourceFile(ver, highestCheckedInFile, primes);
            }
        }
        public void Serialize(string path)
        {
            if (version.IsLatest())
            {
                byte[] bytes = new byte[15 + primes.Length * 8];

                bytes[0] = version.major; bytes[1] = version.minor; bytes[2] = version.patch;

                Array.Copy(BitConverter.GetBytes(highestCheckedInFile), 0, bytes, 3, 8);
                Array.Copy(BitConverter.GetBytes(primes.Length), 0, bytes, 11, 4);

                Buffer.BlockCopy(primes, 0, bytes, 15, primes.Length * 8);

                File.WriteAllBytes(path, bytes);
            }
            else if (version.IsEqual(new Version(1, 0, 0)))
            {
                byte[] bytes = new byte[7 + primes.Length * 8];

                bytes[0] = version.major; bytes[1] = version.minor; bytes[2] = version.patch;

                Array.Copy(BitConverter.GetBytes(primes.Length), 0, bytes, 3, 4);

                Buffer.BlockCopy(primes, 0, bytes, 7, primes.Length * 8);

                File.WriteAllBytes(path, bytes);
            }
            else
            {
                throw new IncompatibleVersionException($"Attempted to serialize known primes resource of version {version} but no serialization method was implemented for such version.");
            }
        }



        public static KnownPrimesResourceFile GenerateKnownPrimesResourceFromJobs(PrimeJob[] jobs)
        {
            List<ulong> knownPrimes = new List<ulong>();

            ulong highest = 0;

            foreach (PrimeJob job in jobs)
            {
                knownPrimes.AddRange(job.primes);

                if (job.start + job.count > highest)
                    highest = job.start + job.count;
            }

            return new KnownPrimesResourceFile(KnownPrimesResourceFile.Version.latest, highest, knownPrimes.ToArray());
        }



        public struct Version
        {
            public readonly byte major, minor, patch;



            public static Version zero = new Version(0, 0, 0);
            public static Version latest = new Version(1, 1, 0);
            public static Version[] compatible = new Version[] { new Version(1, 0, 0), new Version(1, 1, 0) };



            public Version(byte major, byte minor, byte patch)
            {
                this.major = major; this.minor = minor; this.patch = patch;
            }



            public override string ToString()
            {
                return $"v{major}.{minor}.{patch}";
            }
            public static bool IsEqual(Version a, Version b)
            {
                if (a.major == b.major && a.minor == b.minor && a.patch == b.patch)
                    return true;
                return false;
            }
            public bool IsEqual(Version a)
            {
                return IsEqual(this, a);
            }
            public static bool IsLatest(Version ver)
            {
                return IsEqual(Version.latest, ver);
            }
            public bool IsLatest()
            {
                return IsEqual(Version.latest, this);
            }
            public static bool IsCompatible(Version ver)
            {
                return compatible.Contains(ver);
            }
            public bool IsCompatible()
            {
                return compatible.Contains(this);
            }
        }



        public class IncompatibleVersionException : Exception
        {
            public IncompatibleVersionException() : base() { }
            public IncompatibleVersionException(string message) : base(message) { }
        }
    }

    public class PrimeJob
    {
        public enum Status
        {
            Not_started,
            Started,
            Finished,
            None
        }



        public readonly Version version;
        public readonly uint batch;
        public readonly ulong start, count;
        public ulong progress; //0 = not started, xxx = progress, count = done, ulong.MaxValue = error
        public List<ulong> primes;



        public static PrimeJob empty = new PrimeJob(Version.zero, 0, 0, 0, new List<ulong>());



        public PrimeJob(Version version, ulong start, ulong count)
        {
            this.version = version; batch = 0; this.start = start; this.count = count; progress = 0; this.primes = new List<ulong>();
        }
        public PrimeJob(Version version, ulong start, ulong count, ulong progress, ref ulong[] primes)
        {
            this.version = version; batch = 0; this.start = start; this.count = count; this.progress = progress; this.primes = primes.ToList();
        }
        public PrimeJob(Version version, ulong start, ulong count, ulong progress, List<ulong> primes)
        {
            this.version = version; batch = 0; this.start = start; this.count = count; this.progress = progress; this.primes = primes;
        }
        public PrimeJob(Version version, uint batch, ulong start, ulong count)
        {
            this.version = version; this.batch = batch; this.start = start; this.count = count; progress = 0; this.primes = new List<ulong>();
        }
        public PrimeJob(Version version, uint batch, ulong start, ulong count, ulong progress, ref ulong[] primes)
        {
            this.version = version; this.batch = batch; this.start = start; this.count = count; this.progress = progress; this.primes = primes.ToList();
        }
        public PrimeJob(Version version, uint batch, ulong start, ulong count, ulong progress, List<ulong> primes)
        {
            this.version = version; this.batch = batch; this.start = start; this.count = count; this.progress = progress; this.primes = primes;
        }



        public static void Deserialize(string path, out PrimeJob job)
        {
            /* v1.0.0
            * 3 bytes          Version     version (1 byte major, 1 byte minor, 1 byte patch)
            * 8 bytes          ulong       start
            * 8 bytes          ulong       count
            * 8 bytes          ulong       progress
            * 4 bytes          int         primesInFile
            * xxx              ulong[]     primes
            */
            /* v1.0.1
            * 3 bytes          Version     version (1 byte major, 1 byte minor, 1 byte patch)
            * 4 bytes          uint        batch
            * 8 bytes          ulong       start
            * 8 bytes          ulong       count
            * 8 bytes          ulong       progress
            * 4 bytes          int         primesInFile
            * xxx              ulong[]     primes
            */

            job = new PrimeJob(Version.zero, 0, 0);
            byte[] bytes = File.ReadAllBytes(path);

            Version ver = new Version(bytes[0], bytes[1], bytes[2]);

            if (!ver.IsCompatible())
            {
                throw new IncompatibleVersionException($"Attempted to deserialize job of version {ver} but no serialization method was implemented for such version.");
            }
            else if (!ver.IsLatest())
            {
                if (ver.IsEqual(new Version(1, 0, 0)))
                {
                    ulong start = BitConverter.ToUInt64(bytes, 3);
                    ulong count = BitConverter.ToUInt64(bytes, 11);
                    ulong progress = BitConverter.ToUInt64(bytes, 19);

                    int primesInFile = BitConverter.ToInt32(bytes, 27);

                    ulong[] primes = new ulong[primesInFile];
                    Buffer.BlockCopy(bytes, 31, primes, 0, primesInFile * 8);

                    job = new PrimeJob(ver, start, count, progress, ref primes);
                }
            }
            else //it is latest
            {
                uint batch = BitConverter.ToUInt32(bytes, 3);
                ulong start = BitConverter.ToUInt64(bytes, 7);
                ulong count = BitConverter.ToUInt64(bytes, 15);
                ulong progress = BitConverter.ToUInt64(bytes, 23);

                int primesInFile = BitConverter.ToInt32(bytes, 31);

                ulong[] primes = new ulong[primesInFile];
                Buffer.BlockCopy(bytes, 35, primes, 0, primesInFile * 8);

                job = new PrimeJob(ver, batch, start, count, progress, ref primes);
            }
        }
        public static void PeekProgressFromFile(string path, out Status status)
        {
            status = Status.None;
            byte[] allBytes = File.ReadAllBytes(path);

            Version ver = new Version(allBytes[0], allBytes[1], allBytes[2]);

            if (!ver.IsCompatible())
            {
                throw new IncompatibleVersionException($"Attempted to peek progress from job of version {ver} but no serialization method was implemented for such version.");
            }
            else if (!ver.IsLatest())
            {
                if (ver.IsEqual(new Version(1, 0, 0)))
                {
                    byte[] bytes = new byte[16];
                    Array.Copy(allBytes, 11, bytes, 0, 16);

                    ulong count = BitConverter.ToUInt64(bytes, 0);
                    ulong progress = BitConverter.ToUInt64(bytes, 8);

                    if (progress == 0)
                        status = Status.Not_started;
                    else if (progress == count)
                        status = Status.Finished;
                    else
                        status = Status.Started;
                }
            }
            else //it is latest
            {
                byte[] bytes = new byte[16];
                Array.Copy(allBytes, 15, bytes, 0, 16);

                ulong count = BitConverter.ToUInt64(bytes, 0);
                ulong progress = BitConverter.ToUInt64(bytes, 8);

                if (progress == 0)
                    status = Status.Not_started;
                else if (progress == count)
                    status = Status.Finished;
                else
                    status = Status.Started;
            }
        }
        public void Serialize(string path)
        {
            if (version.IsLatest())
            {
                byte[] bytes = new byte[35 + primes.Count * 8];

                bytes[0] = version.major; bytes[1] = version.minor; bytes[2] = version.patch;

                Array.Copy(BitConverter.GetBytes(batch), 0, bytes, 3, 4);
                Array.Copy(BitConverter.GetBytes(start), 0, bytes, 7, 8);
                Array.Copy(BitConverter.GetBytes(count), 0, bytes, 15, 8);
                Array.Copy(BitConverter.GetBytes(progress), 0, bytes, 23, 8);
                Array.Copy(BitConverter.GetBytes(primes.Count), 0, bytes, 31, 4);

                Buffer.BlockCopy(primes.ToArray(), 0, bytes, 35, primes.Count * 8);

                File.WriteAllBytes(path, bytes);
            }
            else if (version.IsEqual(new Version(1, 0, 0)))
            {
                byte[] bytes = new byte[31 + primes.Count * 8];

                bytes[0] = version.major; bytes[1] = version.minor; bytes[2] = version.patch;

                Array.Copy(BitConverter.GetBytes(start), 0, bytes, 3, 8);
                Array.Copy(BitConverter.GetBytes(count), 0, bytes, 11, 8);
                Array.Copy(BitConverter.GetBytes(progress), 0, bytes, 19, 8);
                Array.Copy(BitConverter.GetBytes(primes.Count), 0, bytes, 27, 4);

                Buffer.BlockCopy(primes.ToArray(), 0, bytes, 31, primes.Count * 8);

                File.WriteAllBytes(path, bytes);
            }
            else
            {
                throw new IncompatibleVersionException($"Attempted to serialize job of version {version} but no serialization method was implemented for such version. IsCompatible={version.IsCompatible()}.");
            }
        }



        public static PrimeJob[] GenerateJobs(ulong start, ulong countPerJob, ulong jobCount, uint startingBatch, int jobsPerBatch)
        {
            PrimeJob[] jobs = new PrimeJob[jobCount];

            uint b = startingBatch;
            int jobsInBatch = 0;

            for (int i = 0; i < jobs.Length; i++)
            {
                jobs[i] = new PrimeJob(PrimeJob.Version.latest, b, start + (countPerJob * (ulong)i), countPerJob);

                jobsInBatch++;

                if (jobsInBatch >= jobsPerBatch)
                {
                    jobsInBatch = 0;
                    b++;
                }
            }

            return jobs;
        }
        public static bool CheckJob(ref PrimeJob job, bool cleanDuplicates, out string message)
        {
            message = string.Empty;

            //Check that header matches content
            if (job.progress > job.count)
                message += "Progress is higher than count\n";


            if (job.primes.Count > 1)
            {
                ulong last = job.primes[0];

                //Check first prime number
                if ((last % 2) == 0 && last != 2) //Check it is odd or two
                    message += $"Prime at index 0 is even. Value {last}\n";

                if (last < job.start) //Check it is within expected range
                    message += $"Prime at index 0 is smaller than job start. Value {last}\n";
                else if (last > job.start + job.count)
                    message += $"Prime at index 0 is greater than job start plus job count. Value {last}\n";


                //Check all of the others
                for (int i = 1; i < job.primes.Count; i++)
                {
                    if (last > job.primes[i]) //Check they are in order
                        message += $"Prime at index {i} smaller than the previous. Value {job.primes[i]}\n";

                    if (last == job.primes[i]) //Check there are no duplicates
                    {
                        if (cleanDuplicates)
                        {
                            job.primes.RemoveAt(i);

                            message += $"Prime at index {i} was duplicated and was fixed. Value {job.primes[i]}\n";
                        }
                        else
                            message += $"Prime at index {i} is duplicated. Value {job.primes[i]}\n";
                    }


                    if ((job.primes[i] % 2) == 0) //Check they are odd (second and higher should never be 2)
                        message += $"Prime at index {i} is even. Value {job.primes[i]}\n";

                    if (job.primes[i] < job.start) //Check they are within expected range
                        message += $"Prime at index {i} is smaller than job start. Value {job.primes[i]}\n";
                    else if (job.primes[i] > job.start + job.count)
                        message += $"Prime at index {i} is greater than job start plus job count. Value {job.primes[i]}\n";

                    last = job.primes[i]; //Update value to check the order
                }
            }

            if (message != string.Empty) return false;
            return true;
        }
        public static void CheckJobsInFolder(string path, out int good, out int bad)
        {
            string[] jobPaths = Utils.GetSubFiles(path, "*.primejob");

            good = 0; bad = 0;

            for (int i = 0; i < jobPaths.Length; i++)
            {
                PrimeJob.Deserialize(jobPaths[i], out PrimeJob job);

                if (PrimeJob.CheckJob(ref job, false, out string _))
                    good++;
                else
                    bad++;
            }
        }
        public static void CleanJobsInFolder(string path, out int good, out int bad)
        {
            string[] jobPaths = Utils.GetSubFiles(path, "*.primejob");

            good = 0; bad = 0;

            for (int i = 0; i < jobPaths.Length; i++)
            {
                PrimeJob.Deserialize(jobPaths[i], out PrimeJob job);

                if (PrimeJob.CheckJob(ref job, true, out string message))
                    good++;
                else
                {
                    if (!message.Contains("fixed"))
                        bad++;
                    else
                        good++;
                }

                job.Serialize(jobPaths[i]);
            }
        }



        public struct Version
        {
            public readonly byte major, minor, patch;



            public static Version zero = new Version(0, 0, 0);
            public static Version latest = new Version(1, 1, 0);
            public static Version[] compatible = new Version[] { new Version(1, 0, 0), new Version(1, 1, 0) };



            public Version(byte major, byte minor, byte patch)
            {
                this.major = major; this.minor = minor; this.patch = patch;
            }



            public override string ToString()
            {
                return $"v{major}.{minor}.{patch}";
            }
            public static bool IsEqual(Version a, Version b)
            {
                if (a.major == b.major && a.minor == b.minor && a.patch == b.patch)
                    return true;
                return false;
            }
            public bool IsEqual(Version a)
            {
                return IsEqual(this, a);
            }
            public static bool IsLatest(Version ver)
            {
                return IsEqual(Version.latest, ver);
            }
            public bool IsLatest()
            {
                return IsEqual(latest);
            }
            public static bool IsCompatible(Version ver)
            {
                return compatible.Contains(ver);
            }
            public bool IsCompatible()
            {
                return compatible.Contains(this);
            }
        }



        public class IncompatibleVersionException: Exception
        {
            public IncompatibleVersionException() : base() { }
            public IncompatibleVersionException(string message) : base(message) { }
        }
    }
}
