namespace Primes.Common.Net.Messages
{
    /// <summary>
    /// Message used to inform the client that the server has acknowledged it's connection and is busy handling other clients.
    /// </summary>
    public class Message_ServerWelcomeWait : Message
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Message_ServerWelcomeWait()
        {
            MessageType = Type.Server_Welcome_Wait;
        }



        /// <summary>
        /// Serializes the given message to a byte array.
        /// </summary>
        /// <returns></returns>
        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType };
        }



        /// <summary>
        /// Deserializes a message from a byte array.
        /// </summary>
        /// <param name="bytes">The byte array containig the message to deserialize.</param>
        /// <returns></returns>
        public static Message_ServerWelcomeWait InternalDeserialize(byte[] bytes) => new Message_ServerWelcomeWait();
    }
}
