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
}
