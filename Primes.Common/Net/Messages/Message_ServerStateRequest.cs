using System;
using System.Text;

namespace Primes.Common.Net.Messages
{
    public class Message_ServerStateRequest : Message
    {
        public string workerId;

        public Message_ServerStateRequest(string workerId)
        {
            MessageType = Type.Server_State_Request;
            this.workerId = workerId;
        }

        public override byte[] Serialize()
        {
            byte[] bytes = new byte[5];
            bytes[0] = (byte)MessageType;
            Array.Copy(Encoding.ASCII.GetBytes(workerId), 0, bytes, 1, 4);
            return bytes;
        }

        public static Message_ServerStateRequest InternalDeserialize(byte[] bytes) => new Message_ServerStateRequest(Encoding.ASCII.GetString(bytes, 1, 4));
    }
}
