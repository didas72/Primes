using System;
using System.Collections.Generic;

using DidasUtils;
using DidasUtils.Logging;
using DidasUtils.Net;

using Primes.Common.Net.Messages;

namespace BatchServer.Modules
{
    public class ClientHandler
    {
        private readonly List<(Client, System.Timers.Timer)> queuedClients;



        public ClientHandler()
        {
            queuedClients = new();
        }



        public void DistributeClient(Client client)
        {
            System.Timers.Timer expireTimer = new();
            expireTimer.Interval = 500;
            expireTimer.Elapsed += ExpireTimer;

            queuedClients.Add((client, expireTimer));
            client.messageReceived += MessageReceived;
            client.StartListening();

            expireTimer.Start();
        }

        private void MessageReceived(Client sender, byte[] data)
        {
            try
            {
                var pair = queuedClients.Find(((Client, System.Timers.Timer) a) => a.Item1 == sender);

                lock (pair.Item1)
                {
                    pair.Item2.Dispose();
                }

                queuedClients.Remove(pair);

                Client cl = pair.Item1;

                Message msg = Message.Deserialize(data);

                if (msg is not Message_Identifier ident) { cl.Disconnect(); return; }

                if (ident.identifier == Message_Identifier.Identifier.Control)
                {
                    Log.LogEvent($"Client '{cl.socket.Client.RemoteEndPoint}' successfully identified as a controller.", "ClientHandler");

                    cl.messageReceived -= MessageReceived;
                    Globals.ctlHandle.Handle(cl);
                }
                else if (ident.identifier == Message_Identifier.Identifier.Client)
                {
                    Log.LogEvent($"Client '{cl.socket.Client.RemoteEndPoint}' successfully identified as a client.", "ClientHandler");

                    cl.messageReceived -= MessageReceived;
                    Globals.server.Handle(cl);
                }
                else
                {
                    Log.LogEvent($"Client '{cl.socket.Client.RemoteEndPoint}' failed to identify. Disconnecting.", "ClientHandler");

                    cl.Disconnect();
                    return;
                }
            }
            catch (Exception e)
            {
                Log.LogException("Error hanlding received message.", "ClientHandler", e);
            }
        }

        private void ExpireTimer(object sender, EventArgs e)
        {
            var pair = queuedClients.Find(((Client, System.Timers.Timer) a) => a.Item2 == sender);

            Log.LogEvent($"Client '{pair.Item1.socket.Client.RemoteEndPoint}' took to long to identify. Disconnecting.", "ClientHandler");

            lock (pair.Item1)
            {
                pair.Item1.Disconnect();
                pair.Item2.Dispose();
            }

            queuedClients.Remove(pair);
        }
    }
}
