namespace Primes.Common.Net.Messages.Old
{
    /// <summary>
    /// Message used to inform the client that the server is closing the connection.
    /// </summary>
    public class Message_ServerCloseConnection : Message
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Message_ServerCloseConnection()
        {
            MessageType = Type.Server_Close_Connection;
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
        public static Message_ServerCloseConnection InternalDeserialize(byte[] bytes) => new Message_ServerCloseConnection();
    }
}
