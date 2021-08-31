using System.Net;

namespace Primes.Updater.Net
{
    public static class Networking
    {
        public static bool TryDownloadFile(string url, string path)
        {
            bool ret = true;

            WebClient client = new WebClient();

            try
            {
                client.DownloadFile("https://github.com/didas72/Primes/releases/download/v1.4.0/Primes.v1.4.0.NET.Framework.4.7.2.7z", "E:/Downloads/tmp_Primes.7z");
            }
            catch
            {
                ret = false;
            }

            client.Dispose();

            return ret;
        }
    }
}
