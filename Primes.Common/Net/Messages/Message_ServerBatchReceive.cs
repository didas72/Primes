namespace Primes.Common.Net.Messages.Old
{
    /// <summary>
    /// Message used to inform the client that the return batches were sucessfully received.
    /// </summary>
    public class Message_ServerBatchReceived : Message
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Message_ServerBatchReceived()
        {
            MessageType = Type.Server_Batch_Received;
        }



        /// <summary>
        /// Serializes the given message to a byte array.
        /// </summary>
        /// <returns></returns>
        public override byte[] Serialize() => new byte[] { (byte)MessageType };



        /// <summary>
        /// Deserializes a message from a byte array.
        /// </summary>
        /// <param name="bytes">The byte array containig the message to deserialize.</param>
        /// <returns></returns>
        public static Message_ServerBatchReceived InternalDeserialize(byte[] bytes) => new Message_ServerBatchReceived();
    }
}
