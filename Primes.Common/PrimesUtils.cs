using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using DidasUtils;

using Primes.Common.Files;

namespace Primes.Common
{
    /// <summary>
    /// Class that contains several genereral-purpose methods.
    /// </summary>
    public static class PrimesUtils
    {
        /// <summary>
        /// Gets a <see cref="Queue{T}"/> with paths of all doable jobs in a directory and it's subdirectories.
        /// </summary>
        /// <param name="path">The full path of the directory to be checked.</param>
        /// <param name="maxCount">The maximum number of jobs to add to the queue.</param>
        /// <param name="sort">Wether or not to sort the jobs by number.</param>
        /// <returns><see cref="Queue{T}"/> with paths of all incomplete jobs in the given directory.</returns>
        public static Queue<string> GetDoableJobs(string path, uint maxCount, bool sort)
        {
            string[] files;
            if (sort)
                files = Utils.GetSubFilesSorted(path, "*.primejob");
            else
                files = Utils.GetSubFiles(path, "*.primejob");

            Queue<string> doableJobs = new Queue<string>();

            for (int i = 0; i < files.Length; i++)
            {
                PrimeJob.Status ret = PrimeJob.PeekStatusFromFile(files[i]);

                if (ret == PrimeJob.Status.Not_started || ret == PrimeJob.Status.Started)
                {
                    doableJobs.Enqueue(files[i]);

                    if (doableJobs.Count >= maxCount) break;
                }
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

            string[] files;
            try
            {
                files = Utils.GetSubFilesSorted(path, "*.primejob");
            }
            catch
            {
                files = Utils.GetSubFiles(path, "*.primejob");
            }

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
        public static bool HasDoableJobs(string path) => GetDoableJob(path, out string _);
    }
}