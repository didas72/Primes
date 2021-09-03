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
        private bool sessionExpired = false;

        private Queue<IMessage> pendingMessageHandles = new Queue<IMessage>();

        private Client client;

        public bool Running { get => serverThread.IsAlive; }



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
            int crashCount = 0;
            int consecutiveCrashes = 0;

            while (doServe)
            {
                try
                {
                    if (Program.clientWaitQueue.TryGetNextClient(out client))
                    {
                        SetupForHandle();
                        if (sessionExpired) continue;

                        Log.LogEvent($"Handling client {client.socket.Client.RemoteEndPoint}.", "ServerThread");

                        HandleClient();

                        Log.LogEvent($"Finished handling client {client.socket.Client.RemoteEndPoint}.", "ServerThread");

                        FinishHandle();
                    }
                    else
                        Thread.Sleep(200);

                    consecutiveCrashes = 0;
                }
                catch (Exception e)
                {
                    crashCount++;
                    consecutiveCrashes++;

                    Log.LogEvent(Log.EventType.Error, $"Error (count {crashCount}t:{consecutiveCrashes}c) in server thread: {e.Message}.", "ServerThread");

                    if (consecutiveCrashes >= Settings.Default.maxConsecutiveCrashes)
                    {
                        Log.LogEvent(Log.EventType.Fatal, $"Reached max consecutive crashes in server thread! ({crashCount}t:{consecutiveCrashes}c).", "ServerThread");
                        break;
                    }
                }
            }
        }



        private IMessage WaitForMessage()
        {
            bool wait = true;
            int waitsLeft = Settings.Default.maxWaitMillis;

            while (wait)
            {
                lock (pendingMessageHandles)
                {
                    wait = pendingMessageHandles.Count == 0;
                }

                Thread.Sleep(1);

                waitsLeft--;

                if (waitsLeft >= 0)
                {
                    ExpiredClient();
                    return null;
                }   
            }

            IMessage message;

            lock (pendingMessageHandles)
            {
                 message = pendingMessageHandles.Dequeue();
            }

            return message;
        }
        private void ExpiredClient()
        {
            client.StopListening();
            Log.LogEvent(Log.EventType.Warning, $"Client {client.socket.Client.RemoteEndPoint} session expired due to timeout.", "ServerThread");
            client.Disconnect();
            client = null;

            lock (pendingMessageHandles)
            {
                pendingMessageHandles.Clear();
            }

            sessionExpired = true;
        }
        private void SendMessage(IMessage message)
        {
            if (!client.SendMessage(message))
            {
                ExpiredClient();
            }
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



        private void SetupForHandle()
        {
            sessionExpired = false;

            lock (pendingMessageHandles)
            {
                pendingMessageHandles.Clear();
            }

            if (!client.socket.Connected)
            {
                ExpiredClient();
                return;
            }

            client.messageReceived = MessageReceivedCallback;
        }
        private void HandleClient()
        {
            try
            {
                SendMessage(new Message_ServerRequestWorkerId());
                if (!(WaitForMessage() is Message_ClientWorkerId workerIdMessage)) return;
                if (sessionExpired) return;

                string workerId = ValidateWorkerId(workerIdMessage.workerId);



                SendMessage(new Message_ServerStateRequest(workerId));
                if (!(WaitForMessage() is Message_ClientRequest clientRequest)) return;
                if (sessionExpired) return;

                Message_ClientRequest.Request requestType = clientRequest.request;
                byte objectCount = clientRequest.objectCount;



                HandleRequest(workerId, requestType, objectCount);
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Error, $"Failed to handle client {client.socket.Client.RemoteEndPoint}: {e.Message}.", "ServerThread");
            }
        }
        private void HandleRequest(string workerId, Message_ClientRequest.Request request, byte objectCount)
        {
            if (request == Message_ClientRequest.Request.NewBatch)
            {
                HandleNewBatchRequest(workerId, objectCount);
            }
            else if (request == Message_ClientRequest.Request.ReturnBatch)
            {
                HandleReturnBatchRequest(workerId, objectCount);
            }
            else
            {
                Log.LogEvent(Log.EventType.Error, $"Unhandled request type {request}.", "ServerThread");
            }
        }
        private void HandleNewBatchRequest(string workerId, byte objectCount)
        {
            int freeWorkerBatchCapacity = Settings.Default.maxBatchesPerWorker - Program.batchTable.FindBatchesAssignedToWorker(workerId, out uint[] _).Length;

            if (freeWorkerBatchCapacity <= 0)//hit max per worker
            {
                SendMessage(new Message_ServerBatchNotAvailable(Message_ServerBatchNotAvailable.Reason.BatchLimitReached));
                return;
            }



            int newBatchMaxCount = Math.Min((int)objectCount, freeWorkerBatchCapacity);
            int[] indexes = Program.batchTable.FindLowestFreeBatches(newBatchMaxCount, out uint[] batchNumbers);

            if (indexes.Length == 0)//none left
            {
                SendMessage(new Message_ServerBatchNotAvailable(Message_ServerBatchNotAvailable.Reason.NoAvailableBatches));
                return;
            }



            if (!CompressAndLoadBatches(batchNumbers, out byte[] objectData))
            {
                SendMessage(new Message_ServerCloseConnection());
                return;
            }
            SendMessage(new Message_ServerBatchSend((byte)indexes.Length, objectData));



            if (!(WaitForMessage() is Message_ClientBatchReceived _)) return;//if client didn't confirm proper data don't apply changes to db  
            if (sessionExpired) return;

            Program.batchTable.AssignBatches(workerId, indexes);
        }
        private void HandleReturnBatchRequest(string workerId, byte objectCount)
        {
            SendMessage(new Message_ServerBatchReturnListening());

            if (!(WaitForMessage() is Message_ClientBatchSend returnedBatches))
            {
                SendMessage(new Message_ServerFailedTransfer());
                return;
            }
            if (sessionExpired) return;

            if (UnloadAndUncompressBatches(returnedBatches.objectData) != returnedBatches.objectCount)
            {
                SendMessage(new Message_ServerFailedTransfer());
                Paths.ClearCache();
                return;
            }
            
            if (!ProcessReceivedBatches(workerId))
            {
                SendMessage(new Message_ServerFailedTransfer());
                Paths.ClearCache();
                return;
            }

            SendMessage(new Message_ServerBatchReceived());
        }
        private void FinishHandle()
        {
            string endpoint = string.Empty;

            try
            {
                if (client.socket.Connected)
                    endpoint = client.socket.Client.RemoteEndPoint.ToString();

                client.StopListening();
                client.messageReceived = null;
                SendMessage(new Message_ServerCloseConnection());
                client.Disconnect();
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Warning, $"Failed to close session with client {endpoint} in a correct way: {e.Message}", "ServerThread");

                client = null;
            }

            Log.LogEvent($"Sucessfully disconnected client {endpoint}.", "ServerThread");
        }



        private bool CompressAndLoadBatches(uint[] batchNumbers, out byte[] bytes)
        {
            bytes = new byte[0];

            try
            {
                List<byte> bytesL = new List<byte>();

                for (int i = 0; i < batchNumbers.Length; i++)
                {
                    byte[] retBytes = CompressAndLoadBatch(batchNumbers[i]);
                    bytesL.AddRange(BitConverter.GetBytes(retBytes.Length));
                    bytesL.AddRange(retBytes);
                }

                bytes = bytesL.ToArray();
                return true;
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Error, $"Failed to compress and load {batchNumbers.Length} batches: {e.Message}.", "ServerThread");
            }

            return false;
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



        private int UnloadAndUncompressBatches(byte[] bytes)
        {
            int count = 0;

            try
            {
                int head = 0;
                Paths.ClearCache();

                while (head < bytes.Length)
                {
                    int len = BitConverter.ToInt32(bytes, head);
                    head += 4 + len;

                    byte[] file = new byte[len];

                    Array.Copy(bytes, head, file, 0, len);

                    UnloadAndUncompress(file, Paths.cachePath);

                    count++;
                }
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Error, $"Failed to unload and uncompress batches: {e.Message}.", "ServerThread");

                return -1;
            }

            return count;
        }
        private void UnloadAndUncompress(byte[] bytes, string path)
        {
            string cachePath = Path.Combine(Paths.cachePath, "tmp.7z");

            File.WriteAllBytes(cachePath, bytes);

            SevenZip.Decompress7z(cachePath, path);
        }



        private bool ProcessReceivedBatches(string workerId)
        {
            string[] batches = Directory.GetFiles(Paths.cachePath);
            uint[] batchNumbers = new uint[batches.Length];
            Dictionary<uint, int> indexes = new Dictionary<uint, int>();

            try
            {
                for (int i = 0; i < batches.Length; i++)
                {
                    batchNumbers[i] = uint.Parse(Path.GetFileNameWithoutExtension(batches[i]));
                }

                indexes = Program.batchTable.FindBatchesOfNumbers(batchNumbers);
                if (!Program.batchTable.AreBatchesAssignedToWorker(workerId, indexes.GetValues(), out bool isAssigned))
                    return false;

                if (!isAssigned)
                    return false;

                Program.batchTable.AssignBatches(workerId, BatchEntry.BatchStatus.Stored_Archived, indexes.GetValues());
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Error, $"Error handling received batches: {e.Message}.", "ServerThread");

                return false;
            }

            for (int i = 0; i < batches.Length; i++)
            {
                try
                {
                    Directory.Move(batches[i], Paths.archivedPath);
                }
                catch (Exception e)
                {
                    Log.LogEvent(Log.EventType.Error, $"Error moving received batch {batchNumbers[i]} from cache to archive: {e.Message}.", "ServerThread");

                    Program.batchTable.AssignBatch("    ", BatchEntry.BatchStatus.Lost, indexes[batchNumbers[i]]);
                }
            }

            foreach (uint i in batchNumbers)
            {
                try
                {
                    Utils.DeleteDirectory(Path.Combine(Paths.sentPath, $"{i}"));
                }
                catch (Exception e)
                {
                    Log.LogEvent(Log.EventType.Error, $"Failed to delete sent batch {i} from storage: {e.Message}.", "ServerThread");
                }
            }

            return true;
        }
    }
}
