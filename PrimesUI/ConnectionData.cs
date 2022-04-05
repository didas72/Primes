using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Primes.UI
{
    internal static class ConnectionData
    {
        public static IPEndPoint RemoteEndpoint { get; set; }
    }
}
