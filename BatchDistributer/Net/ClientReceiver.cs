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
    public class ClientReceiver
    {
        private Thread receiverThread;
        private readonly TcpListener listener;

        private volatile bool doListen = false;



        public ClientReceiver(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
        }



        public void StartListener()
        {
            doListen = true;
            receiverThread = new Thread(ListeningLoop);
            receiverThread.Start();
        }
        public void StopListener()
        {
            doListen = false;
            receiverThread.Join();
        }



        private void ListeningLoop()
        {
            listener.Start();

            while (doListen)
            {
                try
                {
                    if (listener.Pending())
                    {
                        TcpClient socket = listener.AcceptTcpClient();

                        Client client = new Client(socket);

                        InitialAccept(client);

                        Program.clientWaitQueue.EnqueueClient(client);
                    }

                    Thread.Sleep(50);
                }
                catch { }
            }

            listener.Stop();
        }

        private void InitialAccept(Client client)
        {
            client.SendMessage(new Message_ServerWelcomeWait());
            client.StartListening();
        }
    }
}
