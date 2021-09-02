using System;
using System.Text;

namespace Primes.BatchDistributer.Files
{
    public class WorkerClient
    {
        public string Id { get; } // alphanumeric, lower and upper-case, 4 chars long, ASCII encoding
        public TimeStamp LastDeliveredBatch { get; set; }
        public TimeStamp LastReceivedBatch { get; set; }



        public WorkerClient(string id)
        {
            Id = id; 
        }



        public byte[] Serialize()
        {
            byte[] buffer = new byte[20]; //4 id + 8 + 8 timestamps

            Array.Copy(Encoding.ASCII.GetBytes(Id), 0, buffer, 0, 4);
            Array.Copy(LastDeliveredBatch.Serialize(), 0, buffer, 4, 8);
            Array.Copy(LastReceivedBatch.Serialize(), 0, buffer, 12, 8);

            return buffer;
        }
        public static WorkerClient Deserialize(byte[] buffer)
        {
            WorkerClient client = new WorkerClient(Encoding.ASCII.GetString(buffer, 0, 4));

            client.LastDeliveredBatch = new TimeStamp(BitConverter.ToInt64(buffer, 4));
            client.LastReceivedBatch = new TimeStamp(BitConverter.ToInt64(buffer, 12));

            return client;
        }
    }
}
