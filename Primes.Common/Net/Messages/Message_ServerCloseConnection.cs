namespace Primes.Common.Net.Messages
{
    public class Message_ServerCloseConnection : Message
    {
        public Message_ServerCloseConnection()
        {
            MessageType = Type.Server_Close_Connection;
        }

        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType };
        }

        public static Message_ServerCloseConnection InternalDeserialize(byte[] bytes) => new Message_ServerCloseConnection();
    }
}
