namespace Primes.Common.Net.Messages.Old
{
    /// <summary>
    /// Message used to inform the client that the server did receive the batches but they are either corrupted, invalid, rejected or incomplete.
    /// </summary>
    public class Message_ServerFailedTransfer : Message
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Message_ServerFailedTransfer()
        {
            MessageType = Type.Server_Failed_Transfer;
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
        public static Message_ServerFailedTransfer InternalDeserialize(byte[] bytes) => new Message_ServerFailedTransfer();
    }
}
