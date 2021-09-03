using System;
using System.IO;

namespace Primes.BatchDistributer.Files
{
    public static class Paths
    {
        public static string homePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), pendingPath, archivedPath, cachePath, dbPath, sentPath;

        public static void ClearCache()
        {
            foreach (var file in Directory.GetFiles(cachePath))
                File.Delete(file);

            foreach (var dir in Directory.GetDirectories(cachePath))
                Common.Utils.DeleteDirectory(dir);
        }
    }
}
