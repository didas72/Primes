using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Primes.Common;
using Primes.Common.Net;
using Primes.Common.Net.Messages;

namespace Primes.BatchDistributer.Net
{
    public class Server
    {
        private Thread serverThread;

        private volatile bool doServe = false;



        public Server() { }

        

        public void Start()
        {
            doServe = true;
            serverThread = new Thread(ServerLoop);
            serverThread.Start();
        }
        public void Stop()
        {
            doServe = false;
            serverThread.Join();
        }



        private void ServerLoop()
        {
            Client client;

            while (doServe)
            {
                try
                {
                    if (Program.clientWaitQueue.TryGetNextClient(out client))
                    {

                    }
                    else
                        Thread.Sleep(200);
                }
                catch { }
            }
        }
    }
}
