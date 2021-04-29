using System;
using System.IO;
using System.Collections.Generic;

using Primes;
using Primes.Common;
using Primes.Common.Files;

namespace Primes.Common
{
    public static class Utils
    {
        public static Queue<string> GetDoableJobs(string path)
        {
            string[] files = GetSubFiles(path, "*.primejob");

            Queue<string> doableJobs = new Queue<string>();

            for (int i = 0; i < files.Length; i++)
            {
                PrimeJob.Status ret = PrimeJob.PeekStatusFromFile(files[i]);

                if (ret == PrimeJob.Status.Not_started)
                    doableJobs.Enqueue(files[i]);
                else if (ret == PrimeJob.Status.Started)
                    doableJobs.Enqueue(files[i]);
            }

            return doableJobs;
        }
        public static bool HasDoableJobs(string path)
        {
            if (GetSubFiles(path, "*.primejob").Length != 0)
                return true;

            return false;
        }



        public static string[] GetSubFiles(string directory)
        {
            List<string> files = new List<string>();

            foreach (string s in Directory.GetFiles(directory))
                files.AddRange(Directory.GetFiles(s));

            foreach (string d in Directory.GetDirectories(directory))
                files.AddRange(GetSubFiles(d));

            return files.ToArray();
        }
        public static string[] GetSubFiles(string directory, string searchPattern)
        {
            List<string> files = new List<string>();

            foreach (string s in Directory.GetFiles(directory))
                files.AddRange(Directory.GetFiles(s, searchPattern));

            foreach (string d in Directory.GetDirectories(directory))
                files.AddRange(GetSubFiles(d, searchPattern));

            return files.ToArray();
        }
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

            foreach (string s in sorted)
                Console.WriteLine(s);

            return sorted.ToArray();
        }



        public static Queue<T> EnqueueRange<T>(Queue<T> queue, T[] values)
        {
            List<T> f = new List<T>();

            f.AddRange(queue.ToArray());
            f.AddRange(values);

            return new Queue<T>(f.ToArray());
        }
    }
}
