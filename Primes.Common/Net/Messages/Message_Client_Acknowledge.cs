using System;

namespace Primes.Common.Net.Messages
{
    public class Message_Client_Acknowledge : Message
    {
        public byte[] data;



        public Message_Client_Acknowledge(byte[] data)
        {
            MessageType = Type.Client_Acknowledge;
            this.data = data;
        }



        public override byte[] Serialize()
        {
            byte[] ret = new byte[data.Length + 1];

            ret[0] = (byte)MessageType;
            Array.Copy(data, 0, ret, 1, data.Length);

            return ret;
        }



        public static Message_Client_Acknowledge InternalDeserialize(byte[] bytes)
        {
            byte[] data = new byte[bytes.Length - 1];
            Array.Copy(bytes, 1, data, 0, data.Length);

            return new Message_Client_Acknowledge(data);
        }
    }
}
