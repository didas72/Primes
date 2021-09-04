using System;
using System.IO;
using System.Collections.Generic;

using Primes.Common;

namespace Primes.BatchDistributer.Files
{
    public class BatchTable
    {
        private readonly List<BatchEntry> entries;
        public BatchEntry this[int index]
        {
            get => entries[index];
        }



        public BatchTable()
        {
            entries = new List<BatchEntry>();
        }
        public BatchTable(int initialCapacity)
        {
            entries = new List<BatchEntry>(initialCapacity);
        }
        public BatchTable(BatchEntry[] startingValues)
        {
            entries = new List<BatchEntry>(startingValues);
        }



        public void AddEntry(BatchEntry entry)
        {
            lock(entries)
            {
                entries.Add(entry);
            }
        }
        public void AddNewEntry(uint batchNumber, BatchEntry.BatchStatus status)
        {
            lock(entries)
            {
                entries.Add(new BatchEntry(batchNumber, status));
            }
        }



        public bool FindBatchOfNumber(uint batchNumber, out int index)
        {
            bool ret = false;
            index = -1;

            lock (entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].BatchNumber == batchNumber)
                    {
                        index = i;
                    }
                }
            }

            return ret;
        }
        public Dictionary<uint, int> FindBatchesOfNumbers(uint[] batchNumbers)
        {
            Dictionary<uint, int> indexes = new Dictionary<uint, int>();

            lock (entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    for (int j = 0; j < batchNumbers.Length; j++)
                    {
                        if (entries[i].BatchNumber == batchNumbers[j])
                            indexes.Add(batchNumbers[j], i);
                    }
                }
            }

            return indexes;
        }
        public bool FindLowestFreeBatch(out uint batchNumber, out int index)
        {
            batchNumber = uint.MaxValue;
            index = -1;

            lock (entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].BatchNumber < batchNumber && (entries[i].Status == BatchEntry.BatchStatus.Stored_Ready))
                    {
                        batchNumber = entries[i].BatchNumber;
                        index = i;
                    }
                }
            }

            return index != -1;
        }
        public int[] FindLowestFreeBatches(int max, out uint[] batchNumbers)
        {
            List<uint> batchNumbersL = new List<uint>();
            List<int> indexesL = new List<int>();

            lock (entries)
            {
                while (batchNumbersL.Count < max)
                {
                    if (!FindLowestFreeBatch(out uint batchNum, out int index))
                        break;

                    batchNumbersL.Add(batchNum);
                    indexesL.Add(index);
                }
            }

            batchNumbers = batchNumbersL.ToArray();
            return indexesL.ToArray();
        }
        public int[] FindExpiredBatches(DateTime expireTime, out uint[] batchNumbers, out string[] workerIds)
        {
            List<int> indexesL = new List<int>();
            List<uint> batchNumbersL = new List<uint>();
            List<string> workerIdsL = new List<string>();

            lock (entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (!entries[i].LastSent.Equals(new TimeStamp(0)) && entries[i].AssignedWorkerId != "    " && entries[i].LastSent.GetDateTime() < expireTime)
                    {
                        indexesL.Add(i);
                        batchNumbersL.Add(entries[i].BatchNumber);
                        workerIdsL.Add(entries[i].AssignedWorkerId);
                    }
                }

            }

            batchNumbers = batchNumbersL.ToArray();
            workerIds = workerIdsL.ToArray();
            return indexesL.ToArray();
        }



        public int[] FindBatchesAssignedToWorker(string workerId, out uint[] batchNumbers)
        {
            List<int> indexes = new List<int>();
            List<uint> batchNumbersL = new List<uint>();

            lock (entries)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    if (entries[i].AssignedWorkerId == workerId)
                    {
                        indexes.Add(i);
                        batchNumbersL.Add(entries[i].BatchNumber);
                    }
                }
            }

            batchNumbers = batchNumbersL.ToArray();
            return indexes.ToArray();
        }
        public bool IsBatchAssignedToWorker(string workerId, uint batchNumber, out bool isAssigned)
        {
            isAssigned = false;
            bool ret = false;

            lock (entries)
            {
                if (!FindBatchOfNumber(batchNumber, out int index))
                    ret = false;

                ret = IsBatchAssignedToWorker(workerId, index, out isAssigned); 
            }

            return ret;
        }
        public bool IsBatchAssignedToWorker(string workerId, int index, out bool isAssigned)
        {
            isAssigned = false;
            bool ret = false;

            lock (entries)
            {
                if (index >= 0 && index < entries.Count)
                {
                    isAssigned = entries[index].AssignedWorkerId == workerId;

                    ret =  true;
                }
            }
            
            return ret;
        }
        public bool AreBatchesAssignedToWorker(string workerId, uint[] batchNumbers, out bool isAssigned)
        {
            isAssigned = false;
            bool ret = false;

            lock (entries)
            {
                Dictionary<uint, int> indexes = FindBatchesOfNumbers(batchNumbers);

                if (indexes.Count != batchNumbers.Length)
                    ret = false;

                ret = AreBatchesAssignedToWorker(workerId, indexes.GetValues(), out isAssigned);
            }

            return ret;
        }
        public bool AreBatchesAssignedToWorker(string workerId, int[] indexes, out bool isAssigned)
        {
            isAssigned = false;
            bool ret = true;

            lock (entries)
            {
                for (int i = 0; i < indexes.Length; i++)
                {
                    if (indexes[i] < 0 && indexes[i] >= entries.Count)
                    {
                        ret =  false;
                    }
                }

                if (ret)
                {
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        if (entries[indexes[i]].AssignedWorkerId != workerId)
                        {
                            isAssigned = false;
                            ret = false;
                            break;
                        }
                    }
                }
            }

            isAssigned = true;
            return !ret;
        }
        



        public bool AssignBatch(string workerId, int index, TimeSetting timeSetting) => AssignBatch(workerId, BatchEntry.BatchStatus.Sent_Waiting, index, timeSetting);
        public bool AssignBatches(string workerId, int[] indexes, TimeSetting timeSetting) => AssignBatches(workerId, BatchEntry.BatchStatus.Sent_Waiting, indexes, timeSetting);
        public bool AssignBatch(string workerId, BatchEntry.BatchStatus status, int index, TimeSetting timeSetting)
        {
            bool ret = false;

            lock (entries)
            {
                if (index >= 0 && index < entries.Count)
                {
                    entries[index].AssignedWorkerId = workerId;
                    entries[index].Status = status;

                    entries[index] = ApplyTimeSetting(entries[index], timeSetting);

                    ret =  true;
                }
                else
                    ret =  false;
            }

            return ret;
        }
        public bool AssignBatches(string workerId, BatchEntry.BatchStatus status, int[] indexes, TimeSetting timeSetting)
        {
            bool ret = false;

            lock (entries)
            {
                for (int i = 0; i < indexes.Length; i++)
                {
                    if (indexes[i] < 0 && indexes[i] >= entries.Count)
                    {
                        ret = false;
                    }
                }

                if (!ret)
                {
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        entries[indexes[i]].AssignedWorkerId = workerId;
                        entries[indexes[i]].Status = status;

                        entries[indexes[i]] = ApplyTimeSetting(entries[indexes[i]], timeSetting);
                    }
                }
            }

            return ret;
        }



        public byte[] Serialize()
        {
            byte[] buffer = new byte[entries.Count * 25 + 4]; //25 per entry plus 4 header

            Array.Copy(BitConverter.GetBytes(entries.Count), 0, buffer, 0, 4);

            for (int i = 0; i < entries.Count; i++)
            {
                Array.Copy(entries[i].Serialize(), 0, buffer, i * 25 + 4, 25);
            }

            return buffer;
        }
        public static BatchTable Deserialize(byte[] buffer) => Deserialize(buffer, 0);
        public static BatchTable Deserialize(byte[] buffer, int startIndex)
        {
            BatchTable table = new BatchTable(BitConverter.ToInt32(buffer, startIndex));

            for (int i = 0; i < table.entries.Count; i++)
            {
                table.entries[i] = BatchEntry.Deserialize(buffer, i * 25 + 4);
            }

            return table;
        }



        private BatchEntry ApplyTimeSetting(BatchEntry entry, TimeSetting setting)
        {
            switch (setting)
            {
                case TimeSetting.ResetBoth:
                    entry.LastSent = new TimeStamp(0);
                    entry.LastCompleted = new TimeStamp(0);
                    break;

                case TimeSetting.ResetSentUpdateCompleted:
                    entry.LastSent = new TimeStamp(0);
                    entry.LastCompleted = TimeStamp.Now();
                    break;

                case TimeSetting.ResetSentPreserveCompleted:
                    entry.LastSent = new TimeStamp(0);
                    break;

                case TimeSetting.UpdateSentResetCompleted:
                    entry.LastSent = TimeStamp.Now();
                    entry.LastCompleted = new TimeStamp(0);
                    break;

                case TimeSetting.UpdateBoth:
                    entry.LastSent = TimeStamp.Now();
                    entry.LastCompleted = TimeStamp.Now();
                    break;

                case TimeSetting.UpdateSentPreserveCompleted:
                    entry.LastSent = TimeStamp.Now();
                    break;

                case TimeSetting.PreserveSentResetCompleted:
                    entry.LastCompleted = new TimeStamp(0);
                    break;

                case TimeSetting.PreserveSentUpdateCompleted:
                    entry.LastCompleted = TimeStamp.Now();
                    break;

                case TimeSetting.PreserveBoth:
                    break;
            }

            return entry;
        }



        public enum TimeSetting
        {
            ResetBoth,
            ResetSentUpdateCompleted,
            ResetSentPreserveCompleted,
            UpdateSentResetCompleted,
            UpdateBoth,
            UpdateSentPreserveCompleted,
            PreserveSentResetCompleted,
            PreserveSentUpdateCompleted,
            PreserveBoth
        }
    }
}
