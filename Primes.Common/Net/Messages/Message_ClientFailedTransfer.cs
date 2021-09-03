namespace Primes.Common.Net.Messages
{
    public class Message_ClientFailedTransfer : Message
    {
        public Message_ClientFailedTransfer()
        {
            MessageType = Type.Client_Failed_Transfer;
        }

        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType };
        }

        public static Message_ClientFailedTransfer InternalDeserialize(byte[] bytes) => new Message_ClientFailedTransfer();
    }
}
