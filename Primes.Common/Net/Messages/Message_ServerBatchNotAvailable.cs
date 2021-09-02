namespace Primes.Common.Net.Messages
{
    public class Message_ServerBatchNotAvailable : Message
    {
        public Reason reason;

        public Message_ServerBatchNotAvailable(Reason reason)
        {
            this.reason = reason;
            MessageType = Type.Server_Batch_Not_Available;
        }

        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType, (byte)reason };
        }

        public static Message_ServerBatchNotAvailable InternalDeserialize(byte[] bytes) => new Message_ServerBatchNotAvailable((Reason)bytes[1]);

        public enum Reason : byte
        {
            None = 0,
            NotSpecified,
            NoAvailableBatches,
            BatchLimitReached,
        }
    }
}
