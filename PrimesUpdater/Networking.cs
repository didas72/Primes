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
                client.DownloadFile(url, path);
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
