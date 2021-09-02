using System;
using System.Text;

namespace Primes.Common.Net.Messages
{
    public class Message_ClientWorkerId : Message
    {
        public string workerId;

        public Message_ClientWorkerId(string workerId)
        {
            MessageType = Type.Client_WorkerId;
            this.workerId = workerId;
        }

        public override byte[] Serialize()
        {
            byte[] bytes = new byte[5];
            bytes[0] = (byte)MessageType;
            Array.Copy(Encoding.ASCII.GetBytes(workerId), 0, bytes, 1, 4);
            return bytes;
        }

        public static Message_ClientWorkerId InternalDeserialize(byte[] bytes) => new Message_ClientWorkerId(Encoding.ASCII.GetString(bytes, 1, 4));
    }
}
