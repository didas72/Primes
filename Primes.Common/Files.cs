using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Primes.Common.Files
{
    /// <summary>
    /// Memory representation of a KnownPrimesResource file. Provides several methods for creating, serializing and deserializing files. 
    /// </summary>
    public class KnownPrimesResourceFile
    {
        /// <summary>
        /// The file structure version.
        /// </summary>
        public Version FileVersion { get; }
        /// <summary>
        /// The highest number checked when creating the file.
        /// </summary>
        public ulong HighestCheckedInFile { get; set; }
        /// <summary>
        /// Known primes.
        /// </summary>
        public ulong[] Primes { get; set; }



        /// <summary>
        /// Default empty instance.
        /// </summary>
        public static KnownPrimesResourceFile Empty { get; } = new KnownPrimesResourceFile(Version.Zero, 0, new ulong[0]);



        /// <summary>
        /// Initializes a new instance of the <see cref="KnownPrimesResourceFile"/> of the specified version and containing the given primes.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="primes">The primes to be stored.</param>
        public KnownPrimesResourceFile(Version version, ulong[] primes)
        {
            FileVersion = version; if (primes.Length > 0) HighestCheckedInFile = primes.Last(); else HighestCheckedInFile = 0; Primes = primes;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="KnownPrimesResourceFile"/> of the specified version, containing the given primes and keeping the 'highestCheckedInFile' parameter.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="highestCheckedInFile">The highest number checked when creating the file.</param>
        /// <param name="primes">The primes to be stored.</param>
        public KnownPrimesResourceFile(Version version, ulong highestCheckedInFile, ulong[] primes)
        {
            FileVersion = version; HighestCheckedInFile = highestCheckedInFile; Primes = primes;
        }



        /// <summary>
        /// Reads a <see cref="KnownPrimesResourceFile"/> from a file.
        /// </summary>
        /// <param name="path">Path of the file to read from.</param>
        /// <param name="file"><see cref="KnownPrimesResourceFile"/> read from the given path.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        public static void Deserialize(string path, out KnownPrimesResourceFile file)
        {
            file = new KnownPrimesResourceFile(Version.Zero, new ulong[] { 0 });

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
        /// <summary>
        /// Writes a <see cref="KnownPrimesResourceFile"/> to a file.
        /// </summary>
        /// <param name="path">The path to write to.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        public void Serialize(string path)
        {
            if (FileVersion.IsLatest())
            {
                byte[] bytes = new byte[15 + Primes.Length * 8];

                bytes[0] = FileVersion.major; bytes[1] = FileVersion.minor; bytes[2] = FileVersion.patch;

                Array.Copy(BitConverter.GetBytes(HighestCheckedInFile), 0, bytes, 3, 8);
                Array.Copy(BitConverter.GetBytes(Primes.Length), 0, bytes, 11, 4);

                Buffer.BlockCopy(Primes, 0, bytes, 15, Primes.Length * 8);

                File.WriteAllBytes(path, bytes);
            }
            else if (FileVersion.IsEqual(new Version(1, 0, 0)))
            {
                byte[] bytes = new byte[7 + Primes.Length * 8];

                bytes[0] = FileVersion.major; bytes[1] = FileVersion.minor; bytes[2] = FileVersion.patch;

                Array.Copy(BitConverter.GetBytes(Primes.Length), 0, bytes, 3, 4);

                Buffer.BlockCopy(Primes, 0, bytes, 7, Primes.Length * 8);

                File.WriteAllBytes(path, bytes);
            }
            else
            {
                throw new IncompatibleVersionException($"Attempted to serialize known primes resource of version {FileVersion} but no serialization method was implemented for such version.");
            }
        }



        /// <summary>
        /// Generates a <see cref="KnownPrimesResourceFile"/> from an array of <see cref="PrimeJob"/>s.
        /// </summary>
        /// <param name="jobs">The jobs to be used to generate the <see cref="KnownPrimesResourceFile"/>.</param>
        /// <returns><see cref="KnownPrimesResourceFile"/> with the primes from the given <see cref="PrimeJob"/> array.</returns>
        public static KnownPrimesResourceFile GenerateKnownPrimesResourceFromJobs(PrimeJob[] jobs)
        {
            List<ulong> knownPrimes = new List<ulong>();

            ulong highest = 0;

            foreach (PrimeJob job in jobs)
            {
                knownPrimes.AddRange(job.Primes);

                if (job.Start + job.Count > highest)
                    highest = job.Start + job.Count;
            }

            return new KnownPrimesResourceFile(Version.Latest, highest, knownPrimes.ToArray());
        }



        /// <summary>
        /// Struct that represents a file version.
        /// </summary>
        public struct Version
        {
            /// <summary>
            /// Major increment of the version.
            /// </summary>
            public readonly byte major;
            /// <summary>
            /// Minor increment of the version.
            /// </summary>
            public readonly byte minor;
            /// <summary>
            /// Patch increment of the version.
            /// </summary>
            public readonly byte patch;



            /// <summary>
            /// Default zero instance.
            /// </summary>
            public static Version Zero { get; }  = new Version(0, 0, 0);
            /// <summary>
            /// Default latest instance.
            /// </summary>
            public static Version Latest { get; } = new Version(1, 1, 0);
            /// <summary>
            /// Array containing all compatible versions.
            /// </summary>
            public static Version[] Compatible { get; } = new Version[] { new Version(1, 0, 0), new Version(1, 1, 0) };



            /// <summary>
            /// Initializes a new instance of <see cref="Version"/> with the given major, minor and patch increments.
            /// </summary>
            /// <param name="major">Major increment.</param>
            /// <param name="minor">Minor increment.</param>
            /// <param name="patch">Patch increment.</param>
            public Version(byte major, byte minor, byte patch)
            {
                this.major = major; this.minor = minor; this.patch = patch;
            }



            /// <summary>
            /// Converts the <see cref="Version"/> instance to a string representation.
            /// </summary>
            /// <returns>String representing the version.</returns>
            public override string ToString()
            {
                return $"v{major}.{minor}.{patch}";
            }
            /// <summary>
            /// Checks if or not two instances of <see cref="Version"/> have the same value.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns>True if the instances have the same value, false otherwise.</returns>
            public static bool IsEqual(Version a, Version b)
            {
                if (a.major == b.major && a.minor == b.minor && a.patch == b.patch)
                    return true;
                return false;
            }
            /// <summary>
            /// Checks if or not the given <see cref="Version"/> has the same value as the current instance.
            /// </summary>
            /// <param name="a"></param>
            /// <returns>True if the instance has the same value, false otherwise.</returns>
            public bool IsEqual(Version a)
            {
                return IsEqual(this, a);
            }
            /// <summary>
            /// Checks if the given <see cref="Version"/> is the latest one.
            /// </summary>
            /// <param name="ver"></param>
            /// <returns>True if the given instance is the latest, false otherwise.</returns>
            public static bool IsLatest(Version ver)
            {
                return IsEqual(Latest, ver);
            }
            /// <summary>
            /// Checks if the current instance of <see cref="Version"/> is the latest one.
            /// </summary>
            /// <returns>True if the current instance is the latest, false otherwise.</returns>
            public bool IsLatest()
            {
                return IsEqual(Latest, this);
            }
            /// <summary>
            /// Checks if the given <see cref="Version"/> is compatible.
            /// </summary>
            /// <param name="ver"></param>
            /// <returns>True if the given instance is compatible, false otherwise.</returns>
            public static bool IsCompatible(Version ver)
            {
                return Compatible.Contains(ver);
            }
            /// <summary>
            /// Checks if the current <see cref="Version"/> is compatible.
            /// </summary>
            /// <returns>True if the current instance is compatible, false otherwise.</returns>
            public bool IsCompatible()
            {
                return Compatible.Contains(this);
            }
        }



        /// <summary>
        /// Exception thrown when incompatible versions are found.
        /// </summary>
        public class IncompatibleVersionException : Exception
        {
            /// <summary>
            /// Initializes a new instance of <see cref="IncompatibleVersionException"/> with no message.
            /// </summary>
            public IncompatibleVersionException() : base() { }
            /// <summary>
            /// Initializes a new instance of <see cref="IncompatibleVersionException"/> with the given message.
            /// </summary>
            /// <param name="message"></param>
            public IncompatibleVersionException(string message) : base(message) { }
        }
    }

    /// <summary>
    /// Memory representation of a PrimeJob file. Provides several methods for creating, serializing and deserializing files.
    /// </summary>
    public class PrimeJob
    {
        /// <summary>
        /// Enum used to represent the status of a given <see cref="PrimeJob"/>.
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// The given <see cref="PrimeJob"/> has not been started.
            /// </summary>
            Not_started,
            /// <summary>
            /// The given PrimeJob has been started but not finished.
            /// </summary>
            Started,
            /// <summary>
            /// The given PrimeJob is finished.
            /// </summary>
            Finished,
            /// <summary>
            /// Default value.
            /// </summary>
            None
        }



        /// <summary>
        /// The file structure version.
        /// </summary>
        public Version FileVersion { get; }
        /// <summary>
        /// The file compression flags.
        /// </summary>
        public Comp FileCompression { get; }
        /// <summary>
        /// Number used to group PrimeJobs together.
        /// </summary>
        public uint Batch { get; }
        /// <summary>
        /// The first number to be checked.
        /// </summary>
        public ulong Start { get; }
        /// <summary>
        /// The amount of numbers to be checked.
        /// </summary>
        public ulong Count { get; }
        /// <summary>
        /// How many numbers have already been checked.
        /// </summary>
        public ulong Progress { get; set; } //0 = not started, xxx = progress, count = done, ulong.MaxValue = error
        /// <summary>
        /// The prime numbers found in this job.
        /// </summary>
        public List<ulong> Primes { get; set; }



        /// <summary>
        /// Default empty instance.
        /// </summary>
        public static PrimeJob Empty { get; } = new PrimeJob(Version.Zero, 0, 0, 0, new List<ulong>());



        /// <summary>
        /// Initializes a new instance of the <see cref="PrimeJob"/> with the specified version, start and count. Primes defaults to empty, Progress defaults to 0 and Batch defaults to 0.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="start">The first number to be checked.</param>
        /// <param name="count">The amount of numbers to be checked.</param>
        public PrimeJob(Version version, ulong start, ulong count)
        {
            FileVersion = version; Batch = 0; Start = start; Count = count; Progress = 0; Primes = new List<ulong>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimeJob"/> with the specified version, start, count, progress and primes. Batch defaults to 0.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="start">The first number to be checked.</param>
        /// <param name="count">The amount of numbers to be checked.</param>
        /// <param name="progress">The amount of numbers that have already been checked.</param>
        /// <param name="primes">The prime numbers found in this job.</param>
        public PrimeJob(Version version, ulong start, ulong count, ulong progress, ref ulong[] primes)
        {
            FileVersion = version; Batch = 0; Start = start; Count = count; Progress = progress; Primes = primes.ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimeJob"/> with the specified version, start, count, progress and primes. Batch defaults to 0.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="start">The first number to be checked.</param>
        /// <param name="count">The amount of numbers to be checked.</param>
        /// <param name="progress">The amount of numbers that have already been checked.</param>
        /// <param name="primes">The prime numbers found in this job.</param>
        public PrimeJob(Version version, ulong start, ulong count, ulong progress, List<ulong> primes)
        {
            FileVersion = version; Batch = 0; Start = start; Count = count; Progress = progress; Primes = primes;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimeJob"/> with the specified version, start, count, and batch. Progress defaults to 0 and primes to empty.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="batch">The batch to group this file with.</param>
        /// <param name="start">The first number to be checked.</param>
        /// <param name="count">The amount of numbers to be checked.</param>
        public PrimeJob(Version version, uint batch, ulong start, ulong count)
        {
            FileVersion = version; Batch = batch; Start = start; Count = count; Progress = 0; Primes = new List<ulong>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimeJob"/> with the specified version, start, count, batch and primes.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="batch">The batch to group this file with.</param>
        /// <param name="start">The first number to be checked.</param>
        /// <param name="count">The amount of numbers to be checked.</param>
        /// <param name="progress">The amount of numbers that have already been checked.</param>
        /// <param name="primes">The prime numbers found in this job.</param>
        public PrimeJob(Version version, uint batch, ulong start, ulong count, ulong progress, ref ulong[] primes)
        {
            FileVersion = version; Batch = batch; Start = start; Count = count; Progress = progress; Primes = primes.ToList();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimeJob"/> with the specified version, start, count, batch and primes.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="batch">The batch to group this file with.</param>
        /// <param name="start">The first number to be checked.</param>
        /// <param name="count">The amount of numbers to be checked.</param>
        /// <param name="progress">The amount of numbers that have already been checked.</param>
        /// <param name="primes">The prime numbers found in this job.</param>
        public PrimeJob(Version version, uint batch, ulong start, ulong count, ulong progress, List<ulong> primes)
        {
            FileVersion = version; Batch = batch; Start = start; Count = count; Progress = progress; Primes = primes;
        }



        /// <summary>
        /// Reads a <see cref="PrimeJob"/> from a file.
        /// </summary>
        /// <param name="path">Path of the file to read from.</param>
        /// <returns><see cref="PrimeJob"/> read from the given path.</returns>
        /// <exception cref="IncompatibleVersionException">Thrown when attempting to deserialize a <see cref="PrimeJob"/> of an incompatible version.</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        public static PrimeJob Deserialize(string path)
        {
            /* v1.0.0
            * 3 bytes          Version     version (1 byte major, 1 byte minor, 1 byte patch)
            * 8 bytes          ulong       start
            * 8 bytes          ulong       count
            * 8 bytes          ulong       progress
            * 4 bytes          int         primesInFile
            * xxx              ulong[]     primes
            */
            /* v1.1.0
            * 3 bytes          Version     version (1 byte major, 1 byte minor, 1 byte patch)
            * 4 bytes          uint        batch
            * 8 bytes          ulong       start
            * 8 bytes          ulong       count
            * 8 bytes          ulong       progress
            * 4 bytes          int         primesInFile
            * xxx              ulong[]     primes
            */
            /* v1.2.0
            * 3 bytes          Version     version (1 byte major, 1 byte minor, 1 byte patch)
            * 1 byte           Comp        compression header (1 bit compressed, 0bX000000, 1 bit PeakRead compression)
            * 4 bytes          uint        batch
            * 8 bytes          ulong       start
            * 8 bytes          ulong       count
            * 8 bytes          ulong       progress
            * 4 bytes          int         primesInFile
            * xxx              ulong[]     primes
            */

            PrimeJob job = new PrimeJob(Version.Zero, 0, 0);
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

            return job;
        }
        /// <summary>
        /// Checks what the status of a certain <see cref="PrimeJob"/> file is.
        /// </summary>
        /// <param name="path">The path to the file to read from.</param>
        /// <returns><see cref="Status"/> representing the status of the checked <see cref="PrimeJob"/> file.</returns>
        /// <exception cref="IncompatibleVersionException">Thrown when attempting to peek status from a <see cref="PrimeJob"/> of an incompatible version.</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        public static Status PeekStatusFromFile(string path)
        {
            Status status = Status.None;
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

            return status;
        }
        /// <summary>
        /// Writes a <see cref="PrimeJob"/> to a file.
        /// </summary>
        /// <param name="path">The path to write to.</param>
        /// <exception cref="IncompatibleVersionException">Thrown when attempting to write a <see cref="PrimeJob"/> of an incompatible version.</exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        public void Serialize(string path)
        {
            if (FileVersion.IsLatest())
            {
                byte[] bytes = new byte[35 + Primes.Count * 8];

                bytes[0] = FileVersion.major; bytes[1] = FileVersion.minor; bytes[2] = FileVersion.patch;

                Array.Copy(BitConverter.GetBytes(Batch), 0, bytes, 3, 4);
                Array.Copy(BitConverter.GetBytes(Start), 0, bytes, 7, 8);
                Array.Copy(BitConverter.GetBytes(Count), 0, bytes, 15, 8);
                Array.Copy(BitConverter.GetBytes(Progress), 0, bytes, 23, 8);
                Array.Copy(BitConverter.GetBytes(Primes.Count), 0, bytes, 31, 4);

                Buffer.BlockCopy(Primes.ToArray(), 0, bytes, 35, Primes.Count * 8);

                File.WriteAllBytes(path, bytes);
            }
            else if (FileVersion.IsEqual(new Version(1, 0, 0)))
            {
                byte[] bytes = new byte[31 + Primes.Count * 8];

                bytes[0] = FileVersion.major; bytes[1] = FileVersion.minor; bytes[2] = FileVersion.patch;

                Array.Copy(BitConverter.GetBytes(Start), 0, bytes, 3, 8);
                Array.Copy(BitConverter.GetBytes(Count), 0, bytes, 11, 8);
                Array.Copy(BitConverter.GetBytes(Progress), 0, bytes, 19, 8);
                Array.Copy(BitConverter.GetBytes(Primes.Count), 0, bytes, 27, 4);

                Buffer.BlockCopy(Primes.ToArray(), 0, bytes, 31, Primes.Count * 8);

                File.WriteAllBytes(path, bytes);
            }
            else
            {
                throw new IncompatibleVersionException($"Attempted to serialize job of version {FileVersion} but no serialization method was implemented for such version. IsCompatible={FileVersion.IsCompatible()}.");
            }
        }
        /// <summary>
        /// Checks what the status of a certain <see cref="PrimeJob"/> is.
        /// </summary>
        /// <returns><see cref="Status"/> representing the status of the checked <see cref="PrimeJob"/> file.</returns>
        /// <exception cref="IncompatibleVersionException">Thrown when attempting to peek status from a <see cref="PrimeJob"/> of an incompatible version.</exception>
        public Status PeekStatus()
        {
            Status status = Status.None;

            if (!FileVersion.IsCompatible())
            {
                throw new IncompatibleVersionException($"Attempted to peek progress from job of version {FileVersion} but no serialization method was implemented for such version.");
            }
            else if (!FileVersion.IsLatest())
            {
                if (FileVersion.IsEqual(new Version(1, 0, 0)))
                {
                    if (Progress == 0)
                        status = Status.Not_started;
                    else if (Progress == Count)
                        status = Status.Finished;
                    else
                        status = Status.Started;
                }
            }
            else //it is latest
            {
                if (Progress == 0)
                    status = Status.Not_started;
                else if (Progress == Count)
                    status = Status.Finished;
                else
                    status = Status.Started;
            }

            return status;
        }



        /// <summary>
        /// Generates a <see cref="PrimeJob"/> array from the given arguments.
        /// </summary>
        /// <param name="start">The first number to be checked.</param>
        /// <param name="countPerJob">The amount of numbers to be checked per job.</param>
        /// <param name="jobCount">The number of jobs to generate.</param>
        /// <param name="startingBatch">The batch number for the first job batch.</param>
        /// <param name="jobsPerBatch">The number of jobs to be grouped per batch.</param>
        /// <returns><see cref="PrimeJob"/> array containing the generated jobs.</returns>
        /// <exception cref="ArgumentException">Thrown when jobsPerBatch is smaller than one.</exception>
        public static PrimeJob[] GenerateJobs(ulong start, ulong countPerJob, ulong jobCount, uint startingBatch, int jobsPerBatch)
        {
            if (jobsPerBatch < 1)
                throw new ArgumentException("Argument 'jobsPerBatch' must be greater than zero.");

            PrimeJob[] jobs = new PrimeJob[jobCount];

            uint b = startingBatch;
            int jobsInBatch = 0;

            for (int i = 0; i < jobs.Length; i++)
            {
                jobs[i] = new PrimeJob(Version.Latest, b, start + (countPerJob * (ulong)i), countPerJob);

                jobsInBatch++;

                if (jobsInBatch >= jobsPerBatch)
                {
                    jobsInBatch = 0;
                    b++;
                }
            }

            return jobs;
        }
        /// <summary>
        /// Checks if a <see cref="PrimeJob"/> passes basic integrity tests.
        /// </summary>
        /// <param name="job">A reference to the <see cref="PrimeJob"/> to be checked.</param>
        /// <param name="cleanDuplicates">Wether or not to remove duplicated values. (true = remove duplicates)</param>
        /// <param name="message">A string containing the check log.</param>
        /// <returns>A boolean representing if or not the job passed all tests.</returns>
        /// <remarks>This method will check for: parity (except for number 2), order, duplicated values and value in range.</remarks>
        public static bool CheckJob(ref PrimeJob job, bool cleanDuplicates, out string message)
        {
            message = string.Empty;

            //Check that header matches content
            if (job.Progress > job.Count)
                message += "Progress is higher than count\n";


            if (job.Primes.Count > 1)
            {
                ulong last = job.Primes[0];

                //Check first prime number
                if ((last % 2) == 0 && last != 2) //Check it is odd or two
                    message += $"Prime at index 0 is even. Value {last}\n";

                if (last < job.Start) //Check it is within expected range
                    message += $"Prime at index 0 is smaller than job start. Value {last}\n";
                else if (last > job.Start + job.Count)
                    message += $"Prime at index 0 is greater than job start plus job count. Value {last}\n";


                //Check all of the others
                for (int i = 1; i < job.Primes.Count; i++)
                {
                    if (last > job.Primes[i]) //Check they are in order
                        message += $"Prime at index {i} smaller than the previous. Value {job.Primes[i]}\n";

                    if (last == job.Primes[i]) //Check there are no duplicates
                    {
                        if (cleanDuplicates)
                        {
                            job.Primes.RemoveAt(i);

                            message += $"Prime at index {i} was duplicated and was fixed. Value {job.Primes[i]}\n";
                        }
                        else
                            message += $"Prime at index {i} is duplicated. Value {job.Primes[i]}\n";
                    }


                    if ((job.Primes[i] % 2) == 0) //Check they are odd (second and higher should never be 2)
                        message += $"Prime at index {i} is even. Value {job.Primes[i]}\n";

                    if (job.Primes[i] < job.Start) //Check they are within expected range
                        message += $"Prime at index {i} is smaller than job start. Value {job.Primes[i]}\n";
                    else if (job.Primes[i] > job.Start + job.Count)
                        message += $"Prime at index {i} is greater than job start plus job count. Value {job.Primes[i]}\n";

                    last = job.Primes[i]; //Update value to check the order
                }
            }

            if (message != string.Empty) return false;
            return true;
        }
        /// <summary>
        /// Checks all <see cref="PrimeJob"/> files in a directory (and subdirectories) against basic integrity tests.
        /// </summary>
        /// <param name="path">The path of the directory to be checked.</param>
        /// <param name="good">The number of tests that indicate file integrity.</param>
        /// <param name="bad">The number of tests that indicate file corruption.</param>
        public static void CheckJobsInFolder(string path, out int good, out int bad)
        {
            string[] jobPaths = Utils.GetSubFiles(path, "*.primejob");

            good = 0; bad = 0;

            for (int i = 0; i < jobPaths.Length; i++)
            {
                PrimeJob job = Deserialize(jobPaths[i]);

                if (CheckJob(ref job, false, out string _))
                    good++;
                else
                    bad++;
            }
        }
        /// <summary>
        /// Checks and cleans all <see cref="PrimeJob"/> files in a directory (and subdirectories) against basic integrity tests, removing any duplicated values.
        /// </summary>
        /// <param name="path">The path of the directory to be checked.</param>
        /// <param name="good">The number of tests that indicate file integrity.</param>
        /// <param name="bad">The number of tests that indicate file corruption. These may include corrected duplicated values.</param>
        public static void CleanJobsInFolder(string path, out int good, out int bad)
        {
            string[] jobPaths = Utils.GetSubFiles(path, "*.primejob");

            good = 0; bad = 0;

            for (int i = 0; i < jobPaths.Length; i++)
            {
                PrimeJob job = Deserialize(jobPaths[i]);

                if (CheckJob(ref job, true, out string _))
                    good++;
                else
                {
                    bad++;
                }

                job.Serialize(jobPaths[i]);
            }
        }



        /// <summary>
        /// Struct that represents a file verion.
        /// </summary>
        public struct Version
        {
            /// <summary>
            /// Major increment of the version.
            /// </summary>
            public readonly byte major;
            /// <summary>
            /// Minor increment of the version.
            /// </summary>
            public readonly byte minor;
            /// <summary>
            /// Patch increment of the version.
            /// </summary>
            public readonly byte patch;



            /// <summary>
            /// Default zero instance.
            /// </summary>
            public static Version Zero { get; } = new Version(0, 0, 0);
            /// <summary>
            /// Default latest instance.
            /// </summary>
            public static Version Latest { get; } = new Version(1, 1, 0);
            /// <summary>
            /// Array containing all compatible versions.
            /// </summary>
            public static Version[] Compatible { get; } = new Version[] { new Version(1, 0, 0), new Version(1, 1, 0) };



            /// <summary>
            /// Initializes a new instance of <see cref="Version"/> with the given major, minor and patch increments.
            /// </summary>
            /// <param name="major">Major increment.</param>
            /// <param name="minor">Minor increment.</param>
            /// <param name="patch">Patch increment.</param>
            public Version(byte major, byte minor, byte patch)
            {
                this.major = major; this.minor = minor; this.patch = patch;
            }



            /// <summary>
            /// Converts the <see cref="Version"/> instance to a string representation.
            /// </summary>
            /// <returns>String representing the version.</returns>
            public override string ToString()
            {
                return $"v{major}.{minor}.{patch}";
            }
            /// <summary>
            /// Checks if or not two instances of <see cref="Version"/> have the same value.
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns>True if the instances have the same value, false otherwise.</returns>
            public static bool IsEqual(Version a, Version b)
            {
                if (a.major == b.major && a.minor == b.minor && a.patch == b.patch)
                    return true;
                return false;
            }
            /// <summary>
            /// Checks if or not the given <see cref="Version"/> has the same value as the current instance.
            /// </summary>
            /// <param name="a"></param>
            /// <returns>True if the instance has the same value, false otherwise.</returns>
            public bool IsEqual(Version a)
            {
                return IsEqual(this, a);
            }
            /// <summary>
            /// Checks if the given <see cref="Version"/> is the latest one.
            /// </summary>
            /// <param name="ver"></param>
            /// <returns>True if the given instance is the latest, false otherwise.</returns>
            public static bool IsLatest(Version ver)
            {
                return IsEqual(Latest, ver);
            }
            /// <summary>
            /// Checks if the current instance of <see cref="Version"/> is the latest one.
            /// </summary>
            /// <returns>True if the current instance is the latest, false otherwise.</returns>
            public bool IsLatest()
            {
                return IsEqual(Latest, this);
            }
            /// <summary>
            /// Checks if the given <see cref="Version"/> is compatible.
            /// </summary>
            /// <param name="ver"></param>
            /// <returns>True if the given instance is compatible, false otherwise.</returns>
            public static bool IsCompatible(Version ver)
            {
                return Compatible.Contains(ver);
            }
            /// <summary>
            /// Checks if the current <see cref="Version"/> is compatible.
            /// </summary>
            /// <returns>True if the current instance is compatible, false otherwise.</returns>
            public bool IsCompatible()
            {
                return Compatible.Contains(this);
            }
        }



        /// <summary>
        /// Struct that represents a file compression status.
        /// </summary>
        public struct Comp
        {
            public readonly bool IsCompressed;
            public readonly bool PeakReadCompression;



            /// <summary>
            /// Initializes a new instance of <see cref="Comp"/> with the given IsCompressed and PeakReadCompression flags.
            /// </summary>
            /// <param name="IsCompressed">Wether or not the file is compressed.</param>
            /// <param name="PeakReadCompression">Wether or not PeakRead Compression was used.</param>
            public Comp(bool IsCompressed, bool PeakReadCompression)
            {
                this.IsCompressed = IsCompressed; this.PeakReadCompression = PeakReadCompression;
            }

            /// <summary>
            /// Initializes a new instance of <see cref="Comp"/> from a give byte storing the needed flags.
            /// </summary>
            /// <param name="source">Byte containing flags.</param>
            public Comp(byte source)
            {
                IsCompressed = (source & 0b10000000) != 0b10000000;

                PeakReadCompression = (source & 0b00000001) != 0b00000001;
            }
        }



        /// <summary>
        /// Exception thrown when incompatible versions are found.
        /// </summary>
        public class IncompatibleVersionException: Exception
        {
            /// <summary>
            /// Initializes a new instance of <see cref="IncompatibleVersionException"/> with no message.
            /// </summary>
            public IncompatibleVersionException() : base() { }
            /// <summary>
            /// Initializes a new instance of <see cref="IncompatibleVersionException"/> with the given message.
            /// </summary>
            /// <param name="message"></param>
            public IncompatibleVersionException(string message) : base(message) { }
        }
    }
}
