namespace Primes.Common.Net.Messages
{
    /// <summary>
    /// Message used to request the client's stored workerId.
    /// </summary>
    public class Message_ServerRequestWorkerId : Message
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Message_ServerRequestWorkerId()
        {
            MessageType = Type.Server_Request_WorkerId;
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
        public static Message_ServerRequestWorkerId InternalDeserialize(byte[] bytes) => new Message_ServerRequestWorkerId();
    }
}
