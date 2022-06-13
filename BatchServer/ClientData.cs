using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Timers;

using DidasUtils;
using DidasUtils.Logging;

namespace BatchServer
{
    internal class ClientData
    {
        private string wrappedFilePath;
        private string lockPath;
        private int opCount; //keeps track of how many operations have occurred, once a certain value has been reached, update file (reduces file IO)

        private List<Client> clients;
        private List<Batch> batches;



        public int MaxAssignedBatches { get; }
        public int MaxDesyncOps { get; }
        public TimeSpan BatchExpireTime { get; }
        public TimeSpan ClientExpireTime { get; }

        //file structure
        /* 
         * 1) "CLIENTS"
         * 2) clients, table with (uint)clientId, (List<uint>)assignedBatches, (long)lastAccess (for expiring), (long)uniqueId (for log)
         * 3) "BATCHES"
         * 4) batches, table with (uint)batchNum, (byte)status, (long)lastAssigned (or -1)
         * 5) "END"
         * 
         * Notes: clients is the main table, in case of any conflicts, clients retains priority
         *        clients should have their own log, which keeps track of access DateTimes, IPs, operations and any other significant event
         */



        public ClientData(string filePath, bool force, int maxAssignedBatches, int maxDesyncOps, TimeSpan batchExpireTime, TimeSpan clientExpireTime)
        {
            clients = new();
            batches = new();

            opCount = 0;
            MaxAssignedBatches = maxAssignedBatches;
            MaxDesyncOps = maxDesyncOps;
            BatchExpireTime = batchExpireTime;
            ClientExpireTime = clientExpireTime;

            wrappedFilePath = filePath;
            lockPath = Path.Combine(Path.GetDirectoryName(filePath), "cliDta.lock");

            if (File.Exists(lockPath))
            {
                if (!force) throw new Exception("Attempted to open an in-use client data file.");

                Log.LogEvent(Log.EventType.Warning, "Overriding client data lock.", "ClientData");
                File.Delete(lockPath);
            }

            File.Create(lockPath);

            DeserializeFile(); //first time
        }



        public bool ValidateClientId(uint clientId) => clientId == 0 ? false : clients.Any((Client cli) => cli.clientId == clientId);
        //0 if none
        public uint GetNewClientId()
        {
            uint lowest = 0;

            foreach (Client cli in clients)
                if (cli.clientId > lowest) lowest = cli.clientId;

            return ++lowest;
        }
        //ignore safety check, done externally
        public bool BatchLimitReached(uint clientId) => clients.First((Client c) => c.clientId == clientId).assignedBatches.Count >= MaxAssignedBatches;
        //0 if none
        public uint FindFreeBatch() => batches.Any((Batch b) => b.status == Batch.Free) ? batches.First((Batch b) => b.status == Batch.Free).batchNum : 0;
        //ignore safety check, done externally
        public void AssignBatch(uint clientId, uint batchNum)
        {
            Client cli = clients.First((Client c) => c.clientId == clientId);
            Batch b = batches.First((Batch b) => b.batchNum == batchNum);

            cli.assignedBatches.Add(batchNum);
            b.status = Batch.Assigned;

            CountOp();
        }
        //ignore safety check, done externally
        public bool BatchAssignedToClient(uint clientId, uint batchNum) => clients.First((Client c) => c.clientId == clientId).assignedBatches.Contains(batchNum);
        //ignore safety check, done externally
        public void MarkBatchAsComplete(uint clientId, uint batchNum)
        {
            clients.First((Client c) => c.clientId == clientId).assignedBatches.Remove(batchNum);
            batches.First((Batch b) => b.batchNum == batchNum).status = Batch.Complete;

            CountOp();
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

            string line = string.Empty;
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
                        if (line == "END") goto DeserializeFile_end;
                        else batches.Add(Batch.FromString(line));
                        break;
                }
            }

            DeserializeFile_end:

            reader.Close();

            if (line != "END") throw new Exception("Client data is corrupted.");
        }
        private void SerializeFile()
        {
            FileStream fs = File.OpenWrite(wrappedFilePath);
            StreamWriter writer = new(fs);

            writer.WriteLine("CLIENTS");
            foreach (Client cli in clients) writer.WriteLine(cli.ToString());
            writer.WriteLine("BATHCES");
            foreach (Batch b in batches) writer.WriteLine(b.ToString());
            writer.WriteLine("END");

            writer.Flush();
            writer.Close();
        }



        public void Close()
        {
            SerializeFile(); //last time to ensure
            File.Delete(lockPath);
        }



        public void OnExpireElements(object sender, ElapsedEventArgs e)
        {
            ExpireClients();
            ExpireBatches();
            ForceSerialize();
        }
        private void ExpireClients()
        {
            Queue<Client> pendingRemoves = new();

            lock (clients)
            {
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
        }
        private void ExpireBatches()
        {
            lock (batches)
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
        }



        private class Client
        {
            public uint clientId;
            public List<uint> assignedBatches;
            public long lastAccess;
            public long uniqueId;
            public DateTime LastAccessTime { get => DateTime.FromBinary(lastAccess); set => lastAccess = value.ToBinary(); }



            public Client(uint clientId, List<uint> assignedBatches, long lastAccess, long uniqueId)
            {
                this.clientId = clientId;
                this.assignedBatches = assignedBatches;
                this.lastAccess = lastAccess;
                this.uniqueId = uniqueId;
            }



            public static Client FromString(string source)
            {
                //check ToString for format
                string[] parts = source.Split(';');
                if (parts.Length != 4) throw new FormatException();

                uint clientId = uint.Parse(parts[0]);

                string[] subPts = parts[1].Split(','); List<uint> assignedBatches = new();
                for (int i = 0; i < subPts.Length; i++)
                    assignedBatches.Add(uint.Parse(subPts[i]));

                long lastAccess = long.Parse(parts[2]);
                long uniqueId = long.Parse(parts[3]);

                return new(clientId, assignedBatches, lastAccess, uniqueId);
            }
            public override string ToString()
            {
                string ret = $"{clientId};";

                foreach (uint assgn in assignedBatches)
                    ret += $"{assgn},";

                return ret.TrimEnd(',') + $";{lastAccess};{uniqueId}";
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
