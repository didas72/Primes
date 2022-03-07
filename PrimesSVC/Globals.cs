using System;

namespace Primes.SVC
{
    internal static class Globals
    {
        #region Constants
        public const string Company = "Didas72";
        public const string Product = "PrimesSVC";
        #endregion

        public static OS currentOS = OS.None;

        public static string startLogPath;
        public static string homeDir;
        public static string jobsDir;
        public static string completeDir;
        public static string resourcesDir;
        public static string cacheDir;
    }

    internal enum OS
    {
        None = 0,
        Windows,
        Linux,
        OSX,
        FreeBSD,
    }
}
