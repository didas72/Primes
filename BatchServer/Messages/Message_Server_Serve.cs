using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchServer.Messages
{
    public class Message_Server_Serve : Message
    {
        public Message_Server_Serve(int id, Status stat)
        {

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
