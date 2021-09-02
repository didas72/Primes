using System;
using System.IO;
using System.Collections.Generic;

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
