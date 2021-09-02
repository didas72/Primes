namespace Primes.Common.Net.Messages
{
    public class Message_ServerBatchReturnListening : Message
    {
        public Message_ServerBatchReturnListening()
        {
            MessageType = Type.Server_Batch_Return_Listening;
        }

        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType };
        }

        public static Message_ServerBatchReturnListening InternalDeserialize(byte[] bytes) => new Message_ServerBatchReturnListening();
    }
}
