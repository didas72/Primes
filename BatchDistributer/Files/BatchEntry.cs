using System;
using System.Text;

namespace Primes.BatchDistributer.Files
{
    public class BatchEntry
    {
        public uint BatchNumber { get; set; }
        public BatchStatus Status { get; set; }
        public TimeStamp AddedToDataBase { get; private set; }
        public TimeStamp LastSent { get; private set; }
        public TimeStamp LastReceived { get; private set; }
        public TimeStamp LastProgressReported { get; set; }
        public float Progress { get; set; }
        public string WorkerClientId { get; set; }



        public BatchEntry(uint batchNumber, BatchStatus status, TimeStamp addedToDataBase)
        {
            BatchNumber = batchNumber;
            Status = status;
            AddedToDataBase = addedToDataBase;
            Progress = 0f;
        }
        private BatchEntry() { }



        public byte[] Serialize()
        {
            byte[] buffer = new byte[45]; //4 batch num + 1 status + 8 + 8 + 8 + 8 stamps + 4 progress + 4 client ID

            Array.Copy(BitConverter.GetBytes(BatchNumber), 0, buffer, 0, 4);
            buffer[4] = (byte)Status;
            Array.Copy(AddedToDataBase.Serialize(), 0, buffer, 5, 8);
            Array.Copy(LastSent.Serialize(), 0, buffer, 13, 8);
            Array.Copy(LastReceived.Serialize(), 0, buffer, 21, 8);
            Array.Copy(LastProgressReported.Serialize(), 0, buffer, 29, 8);
            Array.Copy(BitConverter.GetBytes(Progress), 0, buffer, 37, 4);
            Array.Copy(Encoding.ASCII.GetBytes(WorkerClientId), 0, buffer, 41, 4);

            return buffer;
        }
        public static BatchEntry Deserialize(byte[] buffer)
        {
            BatchEntry entry = new BatchEntry();

            entry.BatchNumber = BitConverter.ToUInt32(buffer, 0);
            entry.Status = (BatchStatus)buffer[4];
            entry.AddedToDataBase = new TimeStamp(BitConverter.ToInt64(buffer, 5));
            entry.LastSent = new TimeStamp(BitConverter.ToInt64(buffer, 13));
            entry.LastReceived = new TimeStamp();
        }



        public enum BatchStatus : byte
        {
            None = 0,
            Scheduled,
            Ready,
            Sent_Empty,
            Sent_Started,
            Sent_Finished,
            Stored_Empty,
            Stored_Started,
            Stored_Finished,
            Stored_Corrupted,
        }
    }
}
