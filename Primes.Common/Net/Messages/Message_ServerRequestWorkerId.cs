namespace Primes.Common.Net.Messages
{
    public class Message_ServerRequestWorkerId : Message
    {
        public Message_ServerRequestWorkerId()
        {
            MessageType = Type.Server_Request_WorkerId;
        }

        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType };
        }

        public static Message_ServerRequestWorkerId InternalDeserialize(byte[] buffer) => new Message_ServerRequestWorkerId();
    }
}
