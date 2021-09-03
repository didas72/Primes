namespace Primes.Common.Net.Messages
{
    /// <summary>
    /// Message used to confirm to the server that the sent batches were received.
    /// </summary>
    public class Message_ClientBatchReceived : Message
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Message_ClientBatchReceived()
        {
            MessageType = Type.Client_Batch_Received;
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
        public static Message_ClientBatchReceived InternalDeserialize(byte[] bytes) => new Message_ClientBatchReceived();
    }
}