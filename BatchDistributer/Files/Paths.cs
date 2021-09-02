using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Primes.BatchDistributer.Files
{
    public static class Paths
    {
        public static string homePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop), pendingPath, archivedPath, cachePath, dbPath, sentPath;
    }
}
