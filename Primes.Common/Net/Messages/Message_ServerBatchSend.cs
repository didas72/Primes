using System;

namespace Primes.Common.Net.Messages
{
    public class Message_ServerBatchSend : Message
    {
        public byte objectCount;
        public byte[] objectData;

        public Message_ServerBatchSend(byte objectCount, byte[] objectData)
        {
            this.objectCount = objectCount;
            this.objectData = objectData;
            MessageType = Type.Server_Batch_Send;
        }
        private Message_ServerBatchSend() { }

        public override byte[] Serialize()
        {
            byte[] bytes = new byte[2 + objectData.Length]; //1 type + 1 object count + data

            bytes[0] = (byte)MessageType;
            bytes[1] = objectCount;
            Array.Copy(objectData, 0, bytes, 2, objectData.Length);

            return bytes;
        }

        public static Message_ServerBatchSend InternalDeserialize(byte[] bytes)
        {
            Message_ServerBatchSend ret = new Message_ServerBatchSend();
            ret.objectCount = bytes[1];
            Array.Copy(bytes, 2, ret.objectData, 0, bytes.Length - 2);
            return ret;
        }
    }
}
