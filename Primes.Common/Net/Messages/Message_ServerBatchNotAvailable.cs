namespace Primes.Common.Net.Messages
{
    /// <summary>
    /// Message used to inform the client that no batches are available, and optionally provide a reason.
    /// </summary>
    public class Message_ServerBatchNotAvailable : Message
    {
        /// <summary>
        /// The reason why no batches will be sent.
        /// </summary>
        public Reason reason;



        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="reason">The reason why no batches will be sent.</param>
        public Message_ServerBatchNotAvailable(Reason reason)
        {
            this.reason = reason;
            MessageType = Type.Server_Batch_Not_Available;
        }



        /// <summary>
        /// Serializes the given message to a byte array.
        /// </summary>
        /// <returns></returns>
        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType, (byte)reason };
        }



        /// <summary>
        /// Deserializes a message from a byte array.
        /// </summary>
        /// <param name="bytes">The byte array containig the message to deserialize.</param>
        /// <returns></returns>
        public static Message_ServerBatchNotAvailable InternalDeserialize(byte[] bytes) => new Message_ServerBatchNotAvailable((Reason)bytes[1]);



        /// <summary>
        /// Holds the valid reasons for not returning batches.
        /// </summary>
        public enum Reason : byte
        {
            /// <summary>
            /// Default value to prevent the reading of invalid data.
            /// </summary>
            None = 0,
            /// <summary>
            /// Reason could not be specified.
            /// </summary>
            NotSpecified,
            /// <summary>
            /// No unassigned batches are available.
            /// </summary>
            NoAvailableBatches,
            /// <summary>
            /// The client reached it's maximum assigned batches.
            /// </summary>
            BatchLimitReached,
        }
    }
}
