using System;
using System.Net;
using System.Timers;

namespace Primes.UI
{
    internal static class ConnectionData
    {
        public static IPEndPoint RemoteEndpoint { get; set; }
        public static EventHandler OnConnectionCheck { get; set; }
        private static Timer checkConnTimer;



        public static void EnableCheckTimer()
        {
            if (checkConnTimer != null) return;
            checkConnTimer = new Timer()
            {
                Interval = 3000,
                AutoReset = true,
            };
            checkConnTimer.Elapsed += OnTimerElapsed;
            checkConnTimer.Start();
        }
        public static void DisableCheckTimer()
        {
            checkConnTimer?.Stop();
            checkConnTimer = null;
        }

        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            OnConnectionCheck?.Invoke(null, e);
        }
    }
}
