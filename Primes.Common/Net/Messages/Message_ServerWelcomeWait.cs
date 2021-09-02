namespace Primes.Common.Net.Messages
{
    public class Message_ServerWelcomeWait : Message
    {
        public Message_ServerWelcomeWait()
        {
            MessageType = Type.Server_Welcome_Wait;
        }

        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType };
        }

        public static Message_ServerWelcomeWait InternalDeserialize(byte[] bytes) => new Message_ServerWelcomeWait();
    }
}
