using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Timers;

using DidasUtils.Logging;

namespace BatchServer
{
    internal class ClientData
    {
        private readonly string wrappedFilePath;
        private readonly string lockPath;
        private int opCount; //keeps track of how many operations have occurred, once a certain value has been reached, update file (reduces file IO)
        private uint highestAssignedID; //cannot be -1, initialized to 0

        private readonly List<Client> clients;
        private readonly List<Batch> batches;
        private readonly Queue<Action> pendingActions;

        private readonly object _INTERNAL_LOCK_ = new();



        public int MaxAssignedBatches { get; }
        public int MaxDesyncOps { get; }
        public TimeSpan BatchExpireTime { get; }
        public TimeSpan ClientExpireTime { get; }

        //file structure
        /* 
         * 1) "CLIENTS"
         * 2) clients, table with (uint)clientId, (List<uint>)assignedBatches, (long)lastAccess (for expiring)
         * 3) "BATCHES"
         * 4) batches, table with (uint)batchNum, (byte)status, (long)lastAssigned (or -1)
         * 5) "END"
         * 6) (uint) highestAssignedID, always increment assigned IDs, to avoid reusing IDs and having old users return nad interfere with new users
         * 
         * Notes: clients is the main table, in case of any conflicts, clients retains priority
         *        clients should have their own log, which keeps track of access DateTimes, IPs, operations and any other significant event
         */



        public ClientData(string filePath, bool force, int maxAssignedBatches, int maxDesyncOps, TimeSpan batchExpireTime, TimeSpan clientExpireTime)
        {
            clients = new();
            batches = new();
            pendingActions = new();

            opCount = 0;
            MaxAssignedBatches = maxAssignedBatches;
            MaxDesyncOps = maxDesyncOps;
            BatchExpireTime = batchExpireTime;
            ClientExpireTime = clientExpireTime;

            wrappedFilePath = filePath;
            lockPath = Path.ChangeExtension(filePath, ".lock");

            if (File.Exists(lockPath))
            {
                if (!force) throw new Exception("Attempted to open an in-use client data file.");

                Log.LogEvent(Log.EventType.Warning, "Overriding client data lock.", "ClientData");
                File.Delete(lockPath);
            }

            File.Create(lockPath).Close();

            if (!File.Exists(filePath)) //fresh
                File.WriteAllText(filePath, "CLIENTS\nBATCHES\nEND\n0");

            DeserializeFile(); //first time
        }



        public bool ExistsClientId(uint clientId) => clientId != 0 && clients.Any((Client cli) => cli.clientId == clientId);
        //0 if none
        public uint PeekNewClientId()
        {
            return highestAssignedID + 1;
        }
        //ignore safety check, done externally
        public void AddNewClient()
        {
            lock (_INTERNAL_LOCK_)
            {
                Client cli = new(++highestAssignedID, new(), DateTime.Now.Ticks);
                clients.Add(cli);
            }
        }
        //ignore safety check, done externally
        public bool BatchLimitReached(uint clientId) => clients.First((Client c) => c.clientId == clientId).assignedBatches.Count >= MaxAssignedBatches;
        //0 if none
        public uint FindFreeBatch() => batches.Any((Batch b) => b.status == Batch.Free) ? batches.First((Batch b) => b.status == Batch.Free).batchNum : 0;
        //ignore safety check, done externally
        public void AssignBatch(uint clientId, uint batchNum)
        {
            lock (_INTERNAL_LOCK_)
            {
                Client cli = clients.First((Client c) => c.clientId == clientId);
                Batch b = batches.First((Batch b) => b.batchNum == batchNum);

                cli.assignedBatches.Add(batchNum);
                b.status = Batch.Assigned;

                CountOp();
            }
        }
        //ignore safety check, done externally
        public bool BatchAssignedToClient(uint clientId, uint batchNum) => clients.First((Client c) => c.clientId == clientId).assignedBatches.Contains(batchNum);
        //ignore safety check, done externally
        public void MarkBatchAsComplete(uint clientId, uint batchNum)
        {
            lock (_INTERNAL_LOCK_)
            {
                clients.First((Client c) => c.clientId == clientId).assignedBatches.Remove(batchNum);
                batches.First((Batch b) => b.batchNum == batchNum).status = Batch.Complete;

                CountOp();
            }
        }



        public void AddPending(Action act)
        {
            pendingActions.Enqueue(act);
        }
        public void ApplyAllPending()
        {
            lock (_INTERNAL_LOCK_)
            {
                while (pendingActions.Count != 0)
                    pendingActions.Dequeue().Invoke();
            }
        }
        public void DiscardAllPending()
        {
            pendingActions.Clear();
        }



        private void CountOp()
        {
            if (++opCount >= MaxDesyncOps)
            {
                SerializeFile();
                opCount = 0;
            }
        }
        private void ForceSerialize()
        {
            SerializeFile();
            opCount = 0;
        }



        private void DeserializeFile()
        {
            FileStream fs = File.OpenRead(wrappedFilePath);
            StreamReader reader = new(fs);

            string line;
            int stage = 0;

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();

                switch (stage)
                {
                    case 0:
                        if (line != "CLIENTS") throw new Exception("Client data is corrupted.");
                        stage = 1;
                        break;

                    case 1:
                        if (line == "BATCHES") stage = 2;
                        else clients.Add(Client.FromString(line));
                        break;

                    case 2:
                        if (line == "END") stage = 3;
                        else batches.Add(Batch.FromString(line));
                        break;

                    case 3:
                        if (string.IsNullOrWhiteSpace(line))
                            break;
                        highestAssignedID = uint.Parse(line);
                        stage = 99;
                        goto DeserializeFile_end;
                }
            }

            DeserializeFile_end:

            reader.Close();

            if (stage != 99) throw new Exception("Client data is corrupted.");
        }
        private void SerializeFile()
        {
            FileStream fs = File.OpenWrite(wrappedFilePath);
            StreamWriter writer = new(fs);

            writer.WriteLine("CLIENTS");
            foreach (Client cli in clients) writer.WriteLine(cli.ToString());
            writer.WriteLine("BATCHES");
            foreach (Batch b in batches) writer.WriteLine(b.ToString());
            writer.WriteLine("END");
            writer.WriteLine(highestAssignedID.ToString());

            writer.Flush();
            writer.Close();
        }



        public void Close()
        {
            SerializeFile(); //last time to ensure
            File.Delete(lockPath);
        }



        public void OnTimedActions(object sender, ElapsedEventArgs e)
        {
            lock (_INTERNAL_LOCK_)
            {
                ExpireClients();
                ExpireBatches();
                ForceSerialize();
            }
        }
        private void ExpireClients()
        {
            Queue<Client> pendingRemoves = new();

            foreach (Client cli in clients)
            {
                if (cli.LastAccessTime - DateTime.Now > ClientExpireTime)
                {
                    //expire batches first
                    foreach (uint batch in cli.assignedBatches)
                        batches.First((Batch b) => b.batchNum == batch).status = Batch.Free;

                    //queue remove client
                    pendingRemoves.Enqueue(cli);
                }
            }

            while (pendingRemoves.Count > 0) clients.Remove(pendingRemoves.Dequeue());
        }
        private void ExpireBatches()
        {
            foreach (Batch b in batches)
            {
                if (b.LastAssignedTime - DateTime.Now > BatchExpireTime)
                {
                    //update clients first
                    foreach (Client cli in clients)
                    {
                        if (cli.assignedBatches.Contains(b.batchNum))
                        {
                            cli.assignedBatches.Remove(b.batchNum);
                            break;
                        }
                    }

                    //free batch
                    b.status = Batch.Free;
                }
            }
        }



        private class Client
        {
            public uint clientId;
            public List<uint> assignedBatches;
            public long lastAccess;
            public DateTime LastAccessTime { get => DateTime.FromBinary(lastAccess); set => lastAccess = value.ToBinary(); }



            public Client(uint clientId, List<uint> assignedBatches, long lastAccess)
            {
                this.clientId = clientId;
                this.assignedBatches = assignedBatches;
                this.lastAccess = lastAccess;
            }



            public static Client FromString(string source)
            {
                //check ToString for format
                string[] parts = source.Split(';');
                if (parts.Length != 3) throw new FormatException();

                uint clientId = uint.Parse(parts[0]);

                string[] subPts = parts[1].Split(','); List<uint> assignedBatches = new();
                for (int i = 0; i < subPts.Length; i++)
                    assignedBatches.Add(uint.Parse(subPts[i]));

                long lastAccess = long.Parse(parts[2]);

                return new(clientId, assignedBatches, lastAccess);
            }
            public override string ToString()
            {
                string ret = $"{clientId};";

                foreach (uint assgn in assignedBatches)
                    ret += $"{assgn},";

                return ret.TrimEnd(',') + $";{lastAccess}";
            }
        }
        private class Batch
        {
            public uint batchNum;
            public byte status;
            public long lastAssigned;
            public DateTime LastAssignedTime { get => DateTime.FromBinary(lastAssigned); set => lastAssigned = value.ToBinary(); }


            public const byte Invalid = 0;
            public const byte Free = 1;
            public const byte Assigned = 2;
            public const byte Complete = 3;



            public Batch(uint batchNum, byte status, long lastAssigned)
            {
                this.batchNum = batchNum;
                this.status = status;
                this.lastAssigned = lastAssigned;
            }



            public static Batch FromString(string source)
            {
                //check ToString for format
                string[] parts = source.Split(';');
                if (parts.Length != 3) throw new FormatException();

                uint batchNum = uint.Parse(parts[0]);
                byte status = byte.Parse(parts[1]);
                long lastAssigned = long.Parse(parts[2]);

                return new(batchNum, status, lastAssigned);
            }
            public override string ToString()
            {
                return $"{batchNum};{status};{lastAssigned}";
            }
        }
    }
}
