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
            entries.Add(entry);
        }
        public void AddNewEntry(uint batchNumber, BatchEntry.BatchStatus status)
        {
            entries.Add(new BatchEntry(batchNumber, status));
        }



        public bool FindBatchOfNumber(uint batchNumber, out int index)
        {
            index = -1;

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].BatchNumber == batchNumber)
                {
                    index = i;
                    return true;
                }
            }

            return false;
        }
        public Dictionary<uint, int> FindBatchesOfNumbers(uint[] batchNumbers)
        {
            Dictionary<uint, int> indexes = new Dictionary<uint, int>();

            for (int i = 0; i < entries.Count; i++)
            {
                for (int j = 0; j < batchNumbers.Length; j++)
                {
                    if (entries[i].BatchNumber == batchNumbers[j])
                        indexes.Add(batchNumbers[j], i);
                }
            }

            return indexes;
        }
        public bool FindLowestFreeBatch(out uint batchNumber, out int index)
        {
            batchNumber = uint.MaxValue;
            index = -1;

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].BatchNumber < batchNumber && (entries[i].Status == BatchEntry.BatchStatus.Stored_Ready))
                {
                    batchNumber = entries[i].BatchNumber;
                    index = i;
                }
            }

            return index != -1;
        }
        public int[] FindLowestFreeBatches(int max, out uint[] batchNumbers)
        {
            List<uint> batchNumbersL = new List<uint>();
            List<int> indexesL = new List<int>();

            while (batchNumbersL.Count < max)
            {
                if (!FindLowestFreeBatch(out uint batchNum, out int index))
                    break;

                batchNumbersL.Add(batchNum);
                indexesL.Add(index);
            }

            batchNumbers = batchNumbersL.ToArray();
            return indexesL.ToArray();
        }



        public int[] FindBatchesAssignedToWorker(string workerId, out uint[] batchNumbers)
        {
            List<int> indexes = new List<int>();
            List<uint> batchNumbersL = new List<uint>();

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].AssignedWorkerId == workerId)
                {
                    indexes.Add(i);
                    batchNumbersL.Add(entries[i].BatchNumber);
                }
            }

            batchNumbers = batchNumbersL.ToArray();
            return indexes.ToArray();
        }
        public bool IsBatchAssignedToWorker(string workerId, uint batchNumber, out bool isAssigned)
        {
            isAssigned = false;

            if (!FindBatchOfNumber(batchNumber, out int index))
                return false;

            return IsBatchAssignedToWorker(workerId, index, out isAssigned);
        }
        public bool IsBatchAssignedToWorker(string workerId, int index, out bool isAssigned)
        {
            isAssigned = false;

            if (index >= 0 && index < entries.Count)
            {
                isAssigned = entries[index].AssignedWorkerId == workerId;

                return true;
            }
            else
                return false;
        }
        public bool AreBatchesAssignedToWorker(string workerId, uint[] batchNumbers, out bool isAssigned)
        {
            isAssigned = false;

            Dictionary<uint, int> indexes = FindBatchesOfNumbers(batchNumbers);

            if (indexes.Count != batchNumbers.Length)
                return false;

            return AreBatchesAssignedToWorker(workerId, indexes.GetValues(), out isAssigned);
        }
        public bool AreBatchesAssignedToWorker(string workerId, int[] indexes, out bool isAssigned)
        {
            isAssigned = false;

            for (int i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] < 0 && indexes[i] >= entries.Count)
                {
                    return false;
                }
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                if (entries[indexes[i]].AssignedWorkerId != workerId)
                {
                    isAssigned = false;
                    return true;
                }
            }

            isAssigned = true;
            return true;
        }
        



        public bool AssignBatch(string workerId, int index) => AssignBatch(workerId, BatchEntry.BatchStatus.Sent_Waiting, index);
        public bool AssignBatches(string workerId, int[] indexes) => AssignBatches(workerId, BatchEntry.BatchStatus.Sent_Waiting, indexes);
        public bool AssignBatch(string workerId, BatchEntry.BatchStatus status, int index)
        {
            if (index >= 0 && index < entries.Count)
            {
                entries[index].AssignedWorkerId = workerId;
                entries[index].Status = status;

                return true;
            }
            else
                return false;
        }
        public bool AssignBatches(string workerId, BatchEntry.BatchStatus status, int[] indexes)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                if (indexes[i] < 0 && indexes[i] >= entries.Count)
                {
                    return false;
                }
            }

            for (int i = 0; i < indexes.Length; i++)
            {
                entries[indexes[i]].AssignedWorkerId = workerId;
                entries[indexes[i]].Status = status;
            }

            return true;
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
    }
}
