using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

using Primes.Common;
using Primes.Common.Net;
using Primes.Common.Net.Messages;
using Primes.BatchDistributer.Files;


namespace Primes.BatchDistributer.Net
{
    public class Server
    {
        private Thread serverThread;

        private volatile bool doServe = false;

        private Queue<IMessage> pendingMessageHandles = new Queue<IMessage>();



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



        public void MessageReceivedCallback(IMessage message)
        {
            lock (pendingMessageHandles)
            {
                pendingMessageHandles.Enqueue(message);
            }
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
                        HandleClient(client);

                        client.StopListening();
                        client.SendMessage(new Message_ServerCloseConnection());
                        client.Disconnect();
                    }
                    else
                        Thread.Sleep(200);
                }
                catch { }
            }
        }
        private IMessage WaitForMessage()
        {
            bool wait = true;

            while (wait)
            {
                lock (pendingMessageHandles)
                {
                    wait = pendingMessageHandles.Count == 0;
                }

                Thread.Sleep(1);
            }

            IMessage message;

            lock (pendingMessageHandles)
            {
                message = pendingMessageHandles.Dequeue();
            }

            return message;
        }
        private string ValidateWorkerId(string workerId)
        {
            if (workerId == "    ")
            {
                return Program.workerTable.FindLowestFreeWorkerId();
            }

            if (Program.workerTable.FindWorkerWithId(workerId, out int index))
            {
                Program.workerTable.RegisterContactTime(index);

                return workerId;
            }
            else
            {
                return Program.workerTable.FindLowestFreeWorkerId();
            }
        }



        private void HandleClient(Client client)
        {
            client.SendMessage(new Message_ServerRequestWorkerId());
            if (!(WaitForMessage() is Message_ClientWorkerId workerIdMessage))
                return;

            string workerId = ValidateWorkerId(workerIdMessage.workerId);



            client.SendMessage(new Message_ServerStateRequest(workerId));
            if (!(WaitForMessage() is Message_ClientRequest clientRequest))
                return;

            Message_ClientRequest.Request requestType = clientRequest.request;
            byte objectCount = clientRequest.objectCount;



            HandleRequest(client, workerId, requestType, objectCount);
        }
        private void HandleRequest(Client client, string workerId, Message_ClientRequest.Request request, byte objectCount)
        {
            if (request == Message_ClientRequest.Request.NewBatch)
            {
                HandleNewBatchRequest(client, workerId, objectCount);
            }
            else if (request == Message_ClientRequest.Request.ReturnBatch)
            {
                HandleReturnBatchRequest(workerId, objectCount);
            }
        }
        private void HandleNewBatchRequest(Client client, string workerId, byte objectCount)
        {
            int freeBatchCapacity = Settings.Default.maxBatchesPerWorker - Program.batchTable.FindBatchesAssignedToWorker(workerId, out uint[] _).Length;

            if (freeBatchCapacity <= 0)
            {
                client.SendMessage(new Message_ServerBatchNotAvailable(Message_ServerBatchNotAvailable.Reason.BatchLimitReached));
                return;
            }

            int[] indexes = Program.batchTable.FindLowestFreeBatches(freeBatchCapacity, out uint[] batchNumbers);

            if (indexes.Length == 0)
            {
                Program.batchTable.AssignBatches(workerId, indexes);
                client.SendMessage(new Message_ServerBatchNotAvailable(Message_ServerBatchNotAvailable.Reason.NoAvailableBatches));
            }

            byte[] objectData = CompressAndLoadBatches(batchNumbers);

            client.SendMessage(new Message_ServerBatchSend((byte)indexes.Length, objectData));
        }



        private byte[] CompressAndLoadBatches(uint[] batchNumbers)
        {
            List<byte> bytes = new List<byte>();

            for (int i = 0; i < batchNumbers.Length; i++)
            {
                byte[] retBytes = CompressAndLoadBatch(batchNumbers[i]);
                bytes.AddRange(BitConverter.GetBytes(retBytes.Length));
                bytes.AddRange(retBytes);
            }

            return bytes.ToArray();
        }
        private byte[] CompressAndLoadBatch(uint batchNumber)
        {
            string batchPath = Path.Combine(Paths.pendingPath, $"{batchNumber}");

            byte[] bytes =  CompressAndLoad(batchPath);

            Directory.Move(batchPath, Path.Combine(Paths.sentPath, $"{batchNumber}"));

            return bytes;
        }
        private byte[] CompressAndLoad(string path)
        {
            string cachePath = Path.Combine(Paths.cachePath, "tmp.7z");

            SevenZip.Compress7z(path, cachePath);

            byte[] bytes = File.ReadAllBytes(cachePath);

            File.Delete(cachePath);

            return bytes;
        }
    }
}
