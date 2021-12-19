using System;

namespace Primes.Common.Net.Messages
{
    public class Message_Server_Abort : Message
    {
        public Message_Server_Abort()
        {
            MessageType = Type.Server_Abort;
        }

        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType };
        }
    }
}
