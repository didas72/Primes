using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Net;

namespace BatchServer.Modules
{
    public class ConnectionListener
    {
        public bool IsRunning { get => (thread != null && thread.IsAlive); }



        private TcpListener listener;
        private Thread thread;
        private volatile bool run = false;



        public ConnectionListener() { }



        public void StartListener(int port)
        {
            if (IsRunning) return;

            Log.LogEvent("Starting listener.", "ConnectionListener");

            run = true;
            thread = new Thread(() => ListenLoop(port));
            thread.Start();
        }

        public void StopListener()
        {
            Log.LogEvent("Stopping listener.", "ConnectionListener");

            run = false;
            thread?.Join();
        }



        private void ListenLoop(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (run)
            {
                if (!listener.Pending()) { Thread.Sleep(10); continue; }

                TcpClient soc = listener.AcceptTcpClient();

                Log.LogEvent($"Client '{soc.Client.RemoteEndPoint}' connected.", "ConnectionListener");

                Client client = new (soc);
                Globals.clHandle.DistributeClient(client);
            }
        }
    }
}
