using System;
using System.Text;

namespace Primes.BatchDistributer.Files
{
    public class BatchEntry
    {
        public uint BatchNumber { get; set; }
        public BatchStatus Status { get; set; }
        public string AssignedWorkerId { get; set; }
        public TimeStamp LastSent { get; set; }
        public TimeStamp LastCompleted { get; set; }



        public BatchEntry(uint batchNumber, BatchStatus status)
        {
            BatchNumber = batchNumber;
            Status = status;
            AssignedWorkerId = "    ";
            LastSent = new TimeStamp(0);
            LastCompleted = new TimeStamp(0);
        }
        private BatchEntry()
        {

        }



        public byte[] Serialize()
        {
            byte[] buffer = new byte[25]; //4 + 1 + 4 + 8

            Array.Copy(BitConverter.GetBytes(BatchNumber), 0, buffer, 0, 4);
            buffer[4] = (byte)Status;
            Array.Copy(Encoding.ASCII.GetBytes(AssignedWorkerId), 0, buffer, 5, 4);
            Array.Copy(LastSent.Serialize(), 0, buffer, 9, 8);
            Array.Copy(LastCompleted.Serialize(), 0, buffer, 17, 8);

            return buffer;
        }
        public static BatchEntry Deserialize(byte[] buffer) => Deserialize(buffer, 0);
        public static BatchEntry Deserialize(byte[] buffer, int startIndex)
        {
            BatchEntry entry = new BatchEntry
            {
                BatchNumber = BitConverter.ToUInt32(buffer, startIndex),
                Status = (BatchStatus)buffer[startIndex + 4],
                AssignedWorkerId = Encoding.ASCII.GetString(buffer, startIndex + 5, 4),
                LastSent = TimeStamp.Deserialize(buffer, startIndex + 9),
                LastCompleted = TimeStamp.Deserialize(buffer, startIndex + 17)
            };

            return entry;
        }



        public enum BatchStatus : byte
        {
            None = 0,
            Lost,
            Scheduled_Waiting,
            Received_Processing,
            Stored_Ready,
            Sent_Waiting,
            Returned_Processing,
            Stored_Archived,
        }
    }
}
