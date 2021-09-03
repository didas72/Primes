namespace Primes.Common.Net.Messages
{
    public class Message_ServerFailedTransfer : Message
    {
        public Message_ServerFailedTransfer()
        {
            MessageType = Type.Server_Failed_Transfer;
        }

        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType };
        }

        public static Message_ServerFailedTransfer InternalDeserialize(byte[] bytes) => new Message_ServerFailedTransfer();
    }
}
