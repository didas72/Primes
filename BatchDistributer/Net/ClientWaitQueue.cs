﻿using System.Collections.Generic;

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
        public bool TryGetNextClient(out Client client)
        {
            client = new Client();

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
