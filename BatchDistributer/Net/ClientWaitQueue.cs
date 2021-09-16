using System;
using System.Collections.Generic;

using Primes.Common;
using Primes.Common.Net;

namespace Primes.BatchDistributer.Net
{
    public class ClientWaitQueue
    {
        private readonly Queue<Client> clientQueue;



        public ClientWaitQueue()
        {
            clientQueue = new Queue<Client>();
        }



        public void EnqueueClient(Client client)
        {
            lock(clientQueue)
            {
                clientQueue.Enqueue(client);
            }
        }
        public void DisconnectAll()
        {
            Log.LogEvent("Disconnecting all queued clients.", "ClientWaitQueue");

            lock (clientQueue)
            {
                while (clientQueue.Count != 0)
                {
                    try
                    {
                        clientQueue.Dequeue().Disconnect();
                    }
                    catch (Exception e)
                    {
                        Log.LogEvent(Log.EventType.Error, "Failed to disconnect queued client.", "ClientWaitQueue");
                        Log.LogException("Failed to disconnect queued client.", "ClientWaitQueue", e);
                    }
                }
            }
        }
        public bool TryGetNextClient(out Client client)
        {
            client = null;

            lock (clientQueue)
            {
                if (clientQueue.Count != 0)
                {
                    client = clientQueue.Dequeue();
                    return true;
                }
            }

            return false;
        }
    }
}
