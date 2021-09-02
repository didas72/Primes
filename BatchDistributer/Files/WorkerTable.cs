using System;
using System.Collections.Generic;

namespace Primes.BatchDistributer.Files
{
    public class WorkerTable
    {
        private readonly List<Worker> entries;
        public Worker this[int index]
        {
            get => entries[index];
        }



        public WorkerTable()
        {
            entries = new List<Worker>();
        }
        public WorkerTable(int initialCapacity)
        {
            entries = new List<Worker>(initialCapacity);
        }
        public WorkerTable(Worker[] startingValues)
        {
            entries = new List<Worker>(startingValues);
        }



        public void AddEntry(Worker entry)
        {
            entries.Add(entry);
        }
        public void AddNewEntry(string workerId)
        {
            entries.Add(new Worker(workerId));
        }
        public bool RemoveEntry(string workerId)
        {
            if (FindWorkerWithId(workerId, out int index))
            {
                RemoveEntry(index);
                return true;
            }
            else
                return false;
        }
        public bool RemoveEntry(int index)
        {
            if (index > 0 && index < entries.Count)
            {
                entries.RemoveAt(index);
                return true;
            }
            else
                return false;
        }
        public bool RemoveOutdatedUsers(TimeStamp deadline, out string[] workerIds)
        {
            int[] indexes = FindOutdatedUsers(deadline, out workerIds);

            if (indexes.Length != 0)
            {
                for (int i = 0; i < indexes.Length; i++)
                    RemoveEntry(indexes[i]);
                return true;
            }

            return false;
        }



        public bool FindWorkerWithId(string workerId, out int index)
        {
            index = -1;

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Id == workerId)
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }
        public bool FindLastContactedUser(out string workerId, out int index)
        {
            workerId = "    ";
            index = -1;

            DateTime min = DateTime.MinValue;

            for (int i = 0; i < entries.Count; i++)
            {
                DateTime currentTime = entries[i].LastContacted.GetDateTime();

                if (currentTime > min)
                {
                    min = currentTime;
                    workerId = entries[i].Id;
                    index = i;
                }
            }

            return index != -1;
        }
        public string FindLowestFreeWorkerId()
        {
            int highest = 0;
            int val;

            for (int i = 0; i < entries.Count; i++)
            {
                val = Worker.GetWorkerIdValue(entries[i].Id);
                if (val > highest)
                {
                    highest = val;
                }
            }

            return Worker.GetWorkerIdString(highest);
        }
        public int[] FindOutdatedUsers(TimeStamp deadline, out string[] workerIds)
        {
            List<int> indexes = new List<int>();
            List<string> workerIdsL = new List<string>();

            DateTime dead = deadline.GetDateTime();

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].LastContacted.GetDateTime() < dead)
                {
                    indexes.Add(i);
                    workerIdsL.Add(entries[i].Id);
                }
            }

            workerIds = workerIdsL.ToArray();
            return indexes.ToArray();
        }
        public bool RegisterContactTime(string workerId)
        {
            if (FindWorkerWithId(workerId, out int index))
            {
                entries[index].RegisterContactTime();
                return true;
            }
            else
                return false;
        }



        public byte[] Serialize()
        {
            byte[] buffer = new byte[entries.Count * 12 + 4]; //12 per entry plus 4 header

            Array.Copy(BitConverter.GetBytes(entries.Count), 0, buffer, 0, 4);

            for (int i = 0; i < entries.Count; i++)
            {
                Array.Copy(entries[i].Serialize(), 0, buffer, i * 12 + 4, 12);
            }

            return buffer;
        }
        public static WorkerTable Deserialize(byte[] buffer) => Deserialize(buffer, 0);
        public static WorkerTable Deserialize(byte[] buffer, int startIndex)
        {
            WorkerTable table = new WorkerTable(BitConverter.ToInt32(buffer, startIndex));

            for (int i = 0; i < table.entries.Count; i++)
            {
                table.entries[i] = Worker.Deserialize(buffer, i * 12 + 4);
            }

            return table;
        }
    }
}
