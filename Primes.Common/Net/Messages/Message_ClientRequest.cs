namespace Primes.Common.Net.Messages
{
    public class Message_ClientRequest : Message
    {
        public Request request;
        public byte objectCount;

        public Message_ClientRequest(Request request, byte objectCount)
        {
            this.request = request;
            this.objectCount = objectCount;
            MessageType = Type.Client_Request;
        }

        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType, (byte)request, objectCount  };
        }

        public static Message_ClientRequest InternalDeserialize(byte[] bytes) => new Message_ClientRequest((Request)bytes[1], bytes[2]);

        public enum Request : byte
        {
            None = 0,
            NewBatch,
            ReturnBatch,
        }
    }
}
