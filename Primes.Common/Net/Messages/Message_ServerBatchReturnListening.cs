namespace Primes.Common.Net.Messages
{
    /// <summary>
    /// Message used to inform the client that the server is listening for returning batches.
    /// </summary>
    public class Message_ServerBatchReturnListening : Message
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Message_ServerBatchReturnListening()
        {
            MessageType = Type.Server_Batch_Return_Listening;
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
        public static Message_ServerBatchReturnListening InternalDeserialize(byte[] bytes) => new Message_ServerBatchReturnListening();
    }
}
