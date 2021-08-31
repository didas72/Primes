using System;
using System.Collections.Generic;
using System.Threading;

using Primes;
using Primes.Common;
using Primes.Common.Files;
using Primes.Updater.Files;
using Primes.Updater.Net;

namespace Primes.Updater
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Networking.TryDownloadFile("https://github.com/didas72/Primes/releases/download/v1.4.0/Primes.v1.4.0.NET.Framework.4.7.2.7z", "E:\\Downloads\\tmp_Primes.7z"));

            SevenZip.Decompress7z("E:\\Downloads\\tmp_Primes.7z", "E:\\Downloads\\tmp");

            Console.WriteLine("//DONE");
            Console.ReadLine();
        }

        public static string GetLastestReleaseLink()
        {
            //https://github.com/didas72/Primes/releases/latest


        }
    }
}
