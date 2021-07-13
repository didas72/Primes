using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes.Common
{
    /// <summary>
    /// Class that contains several genereral-purpose methods.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Gets a <see cref="Queue{T}"/> with paths of all doable jobs in a directory and it's subdirectories.
        /// </summary>
        /// <param name="path">The full path of the directory to be checked.</param>
        /// <returns><see cref="Queue{T}"/> with paths of all incomplete jobs in the given directory.</returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static Queue<string> GetDoableJobs(string path)
        {
            string[] files = GetSubFiles(path, "*.primejob");

            Queue<string> doableJobs = new Queue<string>();

            for (int i = 0; i < files.Length; i++)
            {
                PrimeJob.Status ret = PrimeJob.PeekStatusFromFile(files[i]);

                if (ret == PrimeJob.Status.Not_started || ret == PrimeJob.Status.Started)
                    doableJobs.Enqueue(files[i]);
            }

            return doableJobs;
        }
        /// <summary>
        /// Gets a string with the path of a doable job in a directory and it's subdirectories.
        /// </summary>
        /// <param name="path">The full path of the directory to be checked.</param>
        /// <param name="jobPath">The full path to the job found.</param>
        /// <returns>True if a doable job is found, false otherwise.</returns>
        public static bool GetDoableJob(string path, out string jobPath)
        {
            jobPath = string.Empty;

            string[] files = GetSubFiles(path, "*.primejob");

            for (int i = 0; i < files.Length; i++)
            {
                PrimeJob.Status ret = PrimeJob.PeekStatusFromFile(files[i]);

                if (ret == PrimeJob.Status.Not_started || ret == PrimeJob.Status.Started)
                {
                    jobPath = files[i];
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Checks if a directory has any doable jobs in it or in it's subdirectories.
        /// </summary>
        /// <param name="path">The full path of the directory to be checked.</param>
        /// <returns>True if there are doable jobs in the given directory, false otherwise.</returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static bool HasDoableJobs(string path)
        {
            if (GetSubFiles(path, "*.primejob").Length != 0)
                return true;

            return false;
        }



        /// <summary>
        /// Gets all files in the given directory and it's subdirectories.
        /// </summary>
        /// <param name="directory">The full path of the directory to be checked.</param>
        /// <returns>Array containing the full paths of every file found.</returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static string[] GetSubFiles(string directory)
        {
            List<string> files = new List<string>();

            foreach (string s in Directory.GetFiles(directory))
                files.AddRange(Directory.GetFiles(s));

            foreach (string d in Directory.GetDirectories(directory))
                files.AddRange(GetSubFiles(d));

            return files.ToArray();
        }
        /// <summary>
        /// Gets all files in the given directory and it's subdirectories that match the given search pattern.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="searchPattern"></param>
        /// <returns>Array containing the full paths of every file found.</returns>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static string[] GetSubFiles(string directory, string searchPattern)
        {
            List<string> files = new List<string>();

            foreach (string f in Directory.GetFiles(directory))
                files.Add(f);

            foreach (string d in Directory.GetDirectories(directory))
                files.AddRange(GetSubFiles(d, searchPattern));

            return files.ToArray();
        }
        /// <summary>
        /// Sorts file paths by their numeric values. Values must be ulong compatible.
        /// </summary>
        /// <param name="filenames">The paths to sort by filename.</param>
        /// <returns>Sorted paths array.</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        public static string[] SortFiles(string[] filenames)
        {
            Dictionary<ulong, string> files = new Dictionary<ulong, string>();
            List<string> sorted = new List<string>();

            for (int i = 0; i < filenames.Length; i++)
            {
                files.Add(ulong.Parse(Path.GetFileNameWithoutExtension(filenames[i])), filenames[i]);
            }

            while (files.Count > 0)
            {
                ulong lowest = ulong.MaxValue;

                foreach (KeyValuePair<ulong, string> pair in files)
                {
                    if (pair.Key < lowest)
                        lowest = pair.Key;
                }

                sorted.Add(files[lowest]);
                files.Remove(lowest);
            }

            return sorted.ToArray();
        }



        /// <summary>
        /// Enqueues multiple items at once.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queue">Reference to the <see cref="Queue{T}"/> to enqueue to.</param>
        /// <param name="values">The values to be enqueued.</param>
        public static void EnqueueRange<T>(ref Queue<T> queue, T[] values)
        {
            List<T> f = new List<T>();

            f.AddRange(queue.ToArray());
            f.AddRange(values);

            queue = new Queue<T>(f);
        }
    }
}
