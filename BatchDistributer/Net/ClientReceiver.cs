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
            Log.LogEvent("Starting ClientReceiver...", "ClientReceiver");

            doListen = true;
            receiverThread = new Thread(ListeningLoop);
            receiverThread.Start();

            Log.LogEvent("ClientReceiver started.", "ClientReceiver");
        }
        public void StopListener()
        {
            Log.LogEvent("Stopping ClientReceiver...", "ClientReceiver");

            doListen = false;
            receiverThread.Join();

            Log.LogEvent("ClientReceiver stopped.", "ClientReceiver");
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

                        Log.LogEvent($"Accepted client {client.socket.Client.RemoteEndPoint}.", "ClientReceiver");
                    }

                    Thread.Sleep(50);
                }
                catch (Exception e)
                {
                    Log.LogEvent(Log.EventType.Error, "Failed to accept client.", "ClientReceiver");
                    Log.LogException("Failed to accept client.", "ClientReceiver", e);
                }
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
