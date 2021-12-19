using System;

namespace Primes.Common.Net.Messages
{
    public class Message_Server_Serve : Message
    {
        public int userId;
        public Status status;



        public Message_Server_Serve(int id, Status stat)
        {
            MessageType = Type.Server_Serve;
            userId = id;
            status = stat;
        }



        public override byte[] Serialize()
        {
            byte[] ret = new byte[6];

            ret[0] = (byte)MessageType;
            Array.Copy(BitConverter.GetBytes(userId), 0, ret, 1, 4);
            ret[5] = (byte)status;

            return ret;
        }



        public static Message_Server_Serve InternalDeserialize(byte[] bytes)
        {
            int id = BitConverter.ToInt32(bytes, 1);

            return new Message_Server_Serve(id, (Status)bytes[5]);
        }



        public enum Status
        {
            /*RequestBatches = 1,
              ReturnBatches = 2,
              ResendBacthes = 3,
              RequestTimeExtend = 4,
            */

            None = 0,
            SendingBatches = 1,
            ListeningReturns = 2,
            ResendingBatches = 3,
            TimeExtending = 4,

            Err_NoAvailableBatches = 5, //no batches here
            Err_MaxBatchLimit = 6,      //you're taking too many batches
            Err_NotAssignedBatches = 7, //returning what I didn't assign?
            Err_NoAssignedBatches = 8,  //resend the 0 assigned batches?
            Err_MaxTimeReached = 9,     //can't extend your time anymore
        }
    }
}
