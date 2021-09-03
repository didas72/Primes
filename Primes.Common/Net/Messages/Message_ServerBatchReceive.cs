namespace Primes.Common.Net.Messages
{
    public class Message_ServerBatchReceived : Message
    {
        public Message_ServerBatchReceived()
        {
            MessageType = Type.Server_Batch_Received;
        }

        public override byte[] Serialize() => new byte[] { (byte)MessageType };

        public static Message_ServerBatchReceived InternalDeserialize(byte[] bytes) => new Message_ServerBatchReceived();
    }
}
