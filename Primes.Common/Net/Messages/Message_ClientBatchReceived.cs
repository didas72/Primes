namespace Primes.Common.Net.Messages
{
    public class Message_ClientBatchReceived : Message
    {
        public Message_ClientBatchReceived()
        {
            MessageType = Type.Client_Batch_Received;
        }

        public override byte[] Serialize() => new byte[] { (byte)MessageType };

        public static Message_ClientBatchReceived InternalDeserialize(byte[] bytes) => new Message_ClientBatchReceived();
    }
}