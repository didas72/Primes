using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Primes.Common.Serializers;

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
        /// The file compression flags.
        /// </summary>
        public Comp FileCompression { get; }
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
        /// Initializes a new instance of the <see cref="KnownPrimesResourceFile"/> of the specified version, containing the given primes and keeping the 'highestCheckedInFile' parameter.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="compression">The file compression method.</param>
        /// <param name="primes">The primes to be stored.</param>
        public KnownPrimesResourceFile(Version version, Comp compression, ulong[] primes)
        {
            FileVersion = version; FileCompression = compression; Primes = primes;
        }



        /// <summary>
        /// Reads a <see cref="KnownPrimesResourceFile"/> from a file.
        /// </summary>
        /// <param name="path">Path of the file to read from.</param>
        /// <returns>Deserialized <see cref="KnownPrimesResourceFile"/></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        public static KnownPrimesResourceFile Deserialize(string path)
        {
            KnownPrimesResourceFile file = new KnownPrimesResourceFile(Version.Zero, new ulong[] { 0 });

            FileStream stream = File.OpenRead(path);

            byte[] verB = new byte[3];
            stream.Read(verB, 0, 3);
            
            Version ver = new Version(verB[0], verB[1], verB[2]);

            if (!ver.IsCompatible())
            {
                throw new IncompatibleVersionException($"Attempted to deserialize known primes resource of version {ver} but no serialization method was implemented for such version.");
            }
            else
            {
                if (ver.IsEqual(new Version(1, 2, 0)))
                {
                    file = KnownPrimesResourceFileSerializer.Deserializev1_2_0(stream);
                }
                else if (ver.IsEqual(new Version(1, 1, 0)))
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    file = KnownPrimesResourceFileSerializer.Deserializev1_1_0(bytes);
                }
                else if (ver.IsEqual(new Version(1, 0, 0)))
                {
                    byte[] bytes = File.ReadAllBytes(path);
                    file = KnownPrimesResourceFileSerializer.Deserializev1_0_0(bytes);
                }
            }

            return file;
        }
        /// <summary>
        /// Writes a <see cref="KnownPrimesResourceFile"/> to a file.
        /// </summary>
        /// <param name="file">Reference to the <see cref="KnownPrimesResourceFile"/> to serialize.</param>
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
        public static void Serialize(KnownPrimesResourceFile file, string path)
        {
            if (!file.FileVersion.IsCompatible())
            {
                throw new IncompatibleVersionException($"Attempted to serialize known primes resource of version {file.FileVersion} but no serialization method was implemented for such version.");
            }
            {
                if (file.FileVersion.IsEqual(new Version(1, 2, 0)))
                {
                    byte[] bytes = KnownPrimesResourceFileSerializer.Serializev1_2_0(file);

                    File.WriteAllBytes(path, bytes);
                }
                else if (file.FileVersion.IsEqual(new Version(1, 1, 0)))
                {
                    byte[] bytes = KnownPrimesResourceFileSerializer.Serializev1_1_0(file);

                    File.WriteAllBytes(path, bytes);
                }
                else if (file.FileVersion.IsEqual(new Version(1, 0, 0)))
                {
                    byte[] bytes = KnownPrimesResourceFileSerializer.Serializev1_0_0(file);

                    File.WriteAllBytes(path, bytes);
                }
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
        public readonly struct Version
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
            public static Version Latest { get; } = new Version(1, 2, 0);
            /// <summary>
            /// Array containing all compatible versions.
            /// </summary>
            public static Version[] Compatible { get; } = new Version[] { new Version(1, 0, 0), new Version(1, 1, 0), new Version(1, 2, 0) };



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
        public readonly struct Comp
        {
            private readonly byte flags;



            /// <summary>
            /// Default value.
            /// </summary>
            public static Comp Default { get; } = new Comp(true, false);



            /// <summary>
            /// Flag for the use of Numerical Chain Compression
            /// </summary>
            public bool NCC { get => (flags & 0b00000010) != 0; }//NNS stands for Numerical Chain Compression
            /// <summary>
            /// Flag for the use of Optimized Number Sequence Storage
            /// </summary>
            public bool ONSS { get => (flags & 0b00000001) != 0; } //aka PeakRead Compression, ONSS stands for Optimized Number Sequence Storage



            /// <summary>
            /// Initializes a new instance of <see cref="Comp"/> with the given IsCompressed and PeakReadCompression flags.
            /// </summary>
            /// <param name="NCC">Wether Numerical Chain Compression was used. <see cref="NCC"/></param>
            /// <param name="ONSS">Wether Optimized Number Sequence Storage was used. <see cref="ONSS"/></param>
            public Comp(bool NCC, bool ONSS)
            {
                flags = 0;
                flags = NCC ? (byte)(flags | 0b00000010) : flags;
                flags = ONSS ? (byte)(flags | 0b00000001) : flags;
            }
            /// <summary>
            /// Initializes a new instance of <see cref="Comp"/> from a give byte containing the needed flags.
            /// </summary>
            /// <param name="source">Byte containing flags.</param>
            public Comp(byte source)
            {
                flags = source;
            }



            /// <summary>
            /// Checks wether or not any compression was used.
            /// </summary>
            /// <returns>True if compression was used, false otherwise.</returns>
            public bool IsCompressed() => NCC || ONSS;
            /// <summary>
            /// Checks wether or not the given <see cref="Comp"/> represents the use of any compression.
            /// </summary>
            /// <param name="comp">The value to check.</param>
            /// <returns>True if compression was used, false otherwise.</returns>
            public static bool IsCompressed(Comp comp) => comp.IsCompressed();
            /// <summary>
            /// Serializes the current <see cref="Comp"/> object.
            /// </summary>
            /// <returns><see cref="byte"/> containing the compression flags.</returns>
            public byte GetByte() => flags;
            /// <summary>
            /// Serializes the given <see cref="Comp"/> object.
            /// </summary>
            /// <param name="comp">The value to serialize.</param>
            /// <returns><see cref="byte"/> containing the compression flags.</returns>
            public static byte GetByte(Comp comp) => comp.GetByte();
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
        public enum Status : byte
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
        /// Initializes a new instance of the <see cref="PrimeJob"/> with the specified version, start and count. Primes defaults to empty, Progress defaults to 0 and Batch defaults to 0.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="start">The first number to be checked.</param>
        /// <param name="count">The amount of numbers to be checked.</param>
        public PrimeJob(Version version, ulong start, ulong count)
        {
            FileVersion = version; FileCompression = Comp.Default; Batch = 0; Start = start; Count = count; Progress = 0; Primes = new List<ulong>();
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
            FileVersion = version; FileCompression = Comp.Default; Batch = 0; Start = start; Count = count; Progress = progress; Primes = primes.ToList();
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
            FileVersion = version; FileCompression = Comp.Default; Batch = 0; Start = start; Count = count; Progress = progress; Primes = primes;
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
            FileVersion = version; FileCompression = Comp.Default; Batch = batch; Start = start; Count = count; Progress = 0; Primes = new List<ulong>();
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
            FileVersion = version; FileCompression = Comp.Default; Batch = batch; Start = start; Count = count; Progress = progress; Primes = primes.ToList();
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
            FileVersion = version; FileCompression = Comp.Default; Batch = batch; Start = start; Count = count; Progress = progress; Primes = primes;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimeJob"/> with the specified version, compression, start, count, batch and primes.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="compression">The file compression method.</param>
        /// <param name="batch">The batch to group this file with.</param>
        /// <param name="start">The first number to be checked.</param>
        /// <param name="count">The amount of numbers to be checked.</param>
        public PrimeJob(Version version, Comp compression, uint batch, ulong start, ulong count)
        {
            FileVersion = version; FileCompression = compression; Batch = batch; Start = start; Count = count; Progress = 0; Primes = new List<ulong>();
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimeJob"/> with the specified version, compression, start, count, batch and primes.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="compression">The file compression method.</param>
        /// <param name="batch">The batch to group this file with.</param>
        /// <param name="start">The first number to be checked.</param>
        /// <param name="count">The amount of numbers to be checked.</param>
        /// <param name="progress">The amount of numbers that have already been checked.</param>
        /// <param name="primes">The prime numbers found in this job.</param>
        public PrimeJob(Version version, Comp compression, uint batch, ulong start, ulong count, ulong progress, List<ulong> primes)
        {
            FileVersion = version; FileCompression = compression; Batch = batch; Start = start; Count = count; Progress = progress; Primes = primes;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimeJob"/> with the specified version, compression, start, count, batch and primes.
        /// </summary>
        /// <param name="version">The file structure version.</param>
        /// <param name="compression">The file compression method.</param>
        /// <param name="batch">The batch to group this file with.</param>
        /// <param name="start">The first number to be checked.</param>
        /// <param name="count">The amount of numbers to be checked.</param>
        /// <param name="progress">The amount of numbers that have already been checked.</param>
        /// <param name="primes">The prime numbers found in this job.</param>
        public PrimeJob(Version version, Comp compression, uint batch, ulong start, ulong count, ulong progress, ref ulong[] primes)
        {
            FileVersion = version; FileCompression = compression; Batch = batch; Start = start; Count = count; Progress = progress; Primes = primes.ToList();
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
            PrimeJob job = new PrimeJob(Version.Zero, 0, 0);

            byte[] bytes = File.ReadAllBytes(path);

            Version ver = new Version(bytes[0], bytes[1], bytes[2]);

            if (!ver.IsCompatible())
            {
                throw new IncompatibleVersionException($"Attempted to deserialize job of version {ver} but no serialization method was implemented for such version.");
            }
            else
            {
                if (ver.IsEqual(new Version(1, 2, 0)))
                {
                    job = PrimeJobSerializer.Deserializev1_2_0(ref bytes);
                }
                else if (ver.IsEqual(new Version(1, 1, 0)))
                {
                    job = PrimeJobSerializer.Deserializev1_1_0(ref bytes);
                }
                else if (ver.IsEqual(new Version(1, 0, 0)))
                {
                    job = PrimeJobSerializer.Deserializev1_0_0(ref bytes);
                }
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
            byte[] bytes = File.ReadAllBytes(path);

            Version ver = new Version(bytes[0], bytes[1], bytes[2]);

            if (!ver.IsCompatible())
            {
                throw new IncompatibleVersionException($"Attempted to peek progress from job of version {ver} but no serialization method was implemented for such version.");
            }
            else if (!ver.IsLatest())
            {
                if (ver.IsEqual(new Version(1, 0, 0)))
                {
                    return PrimeJobSerializer.PeekStatusv1_0_0(ref bytes);
                }
                else if (ver.IsEqual(new Version(1, 1, 0)))
                {
                    return PrimeJobSerializer.PeekStatusv1_1_0(ref bytes);
                }
            }
            else //it is latest
            {
                return PrimeJobSerializer.PeekStatusv1_2_0(ref bytes);
            }

            return Status.None;
        }
        /// <summary>
        /// Writes a <see cref="PrimeJob"/> to a file.
        /// </summary>
        /// <param name="job">The job to serialize.</param>
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
        public static void Serialize(PrimeJob job, string path)
        {
            if (!job.FileVersion.IsCompatible())
            {
                throw new IncompatibleVersionException($"Attempted to deserialize job of version {job.FileVersion} but no serialization method was implemented for such version.");
            }
            else 
            {
                if (job.FileVersion.Equals(new Version(1, 2, 0)))
                {
                    byte[] bytes = PrimeJobSerializer.Serializev1_2_0(job);

                    File.WriteAllBytes(path, bytes);
                }
                else if (job.FileVersion.Equals(new Version(1, 1, 0)))
                {
                    byte[] bytes = PrimeJobSerializer.Serializev1_1_0(job);

                    File.WriteAllBytes(path, bytes);
                }
                else if (job.FileVersion.Equals(new Version(1, 0, 0)))
                {
                    byte[] bytes = PrimeJobSerializer.Serializev1_0_0(job);

                    File.WriteAllBytes(path, bytes);
                }
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
                jobs[i] = new PrimeJob(Version.Latest, new Comp(true, false), b, start + (countPerJob * (ulong)i), countPerJob);

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
        public static bool CheckJob(PrimeJob job, bool cleanDuplicates, out string message)
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

                if (CheckJob(job, false, out string _))
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

                if (CheckJob(job, true, out string _))
                    good++;
                else
                {
                    bad++;
                }

                PrimeJob.Serialize(job, jobPaths[i]);
            }
        }



        /// <summary>
        /// Struct that represents a file verion.
        /// </summary>
        public readonly struct Version
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
            public static Version Latest { get; } = new Version(1, 2, 0);
            /// <summary>
            /// Array containing all compatible versions.
            /// </summary>
            public static Version[] Compatible { get; } = new Version[] { new Version(1, 0, 0), new Version(1, 1, 0), new Version(1, 2, 0) };



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
            /// Checks wether or not two instances of <see cref="Version"/> have the same value.
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
            /// Checks wether or not the given <see cref="Version"/> has the same value as the current instance.
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
        public readonly struct Comp
        {
            private readonly byte flags;



            /// <summary>
            /// Default value.
            /// </summary>
            public static Comp Default { get; } = new Comp(true, false);



            /// <summary>
            /// Flag for the use of Numerical Chain Compression
            /// </summary>
            public bool NCC { get => (flags & 0b00000010) != 0; }//NNS stands for Numerical Chain Compression
            /// <summary>
            /// Flag for the use of Optimized Number Sequence Storage
            /// </summary>
            public bool ONSS { get => (flags & 0b00000001) != 0; } //aka PeakRead Compression, ONSS stands for Optimized Number Sequence Storage



            /// <summary>
            /// Initializes a new instance of <see cref="Comp"/> with the given IsCompressed and PeakReadCompression flags.
            /// </summary>
            /// <param name="NCC">Wether Numerical Chain Compression was used. <see cref="NCC"/></param>
            /// <param name="ONSS">Wether Optimized Number Sequence Storage was used. <see cref="ONSS"/></param>
            public Comp(bool NCC, bool ONSS)
            {
                flags = 0;
                flags = NCC ? (byte)(flags | 0b00000010) : flags;
                flags = ONSS ? (byte)(flags | 0b00000001) : flags;
            }
            /// <summary>
            /// Initializes a new instance of <see cref="Comp"/> from a give byte containing the needed flags.
            /// </summary>
            /// <param name="source">Byte containing flags.</param>
            public Comp(byte source)
            {
                flags = source;
            }



            /// <summary>
            /// Checks wether or not any compression was used.
            /// </summary>
            /// <returns>True if compression was used, false otherwise.</returns>
            public bool IsCompressed() => NCC || ONSS;
            /// <summary>
            /// Checks wether or not the given <see cref="Comp"/> represents the use of any compression.
            /// </summary>
            /// <param name="comp">The value to check.</param>
            /// <returns>True if compression was used, false otherwise.</returns>
            public static bool IsCompressed(Comp comp) => comp.IsCompressed();
            /// <summary>
            /// Serializes the current <see cref="Comp"/> object.
            /// </summary>
            /// <returns><see cref="byte"/> containing the compression flags.</returns>
            public byte GetByte() => flags;
            /// <summary>
            /// Serializes the given <see cref="Comp"/> object.
            /// </summary>
            /// <param name="comp">The value to serialize.</param>
            /// <returns><see cref="byte"/> containing the compression flags.</returns>
            public static byte GetByte(Comp comp) => comp.GetByte();
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
