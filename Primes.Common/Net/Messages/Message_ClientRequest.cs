namespace Primes.Common.Net.Messages.Old
{
    /// <summary>
    /// Message used to tell the server what the client's request is.
    /// </summary>
    public class Message_ClientRequest : Message
    {
        /// <summary>
        /// The request type.
        /// </summary>
        public Request request;
        /// <summary>
        /// The number of batches expected in the coming messages. Used for error checking and request tuning.
        /// </summary>
        public byte objectCount;



        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="request">The request type.</param>
        /// <param name="objectCount">The number of batches expected in the coming messages. Used for error checking and request tuning.</param>
        public Message_ClientRequest(Request request, byte objectCount)
        {
            this.request = request;
            this.objectCount = objectCount;
            MessageType = Type.Client_Request;
        }



        /// <summary>
        /// Serializes the given message to a byte array.
        /// </summary>
        /// <returns></returns>
        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType, (byte)request, objectCount  };
        }



        /// <summary>
        /// Deserializes a message from a byte array.
        /// </summary>
        /// <param name="bytes">The byte array containig the message to deserialize.</param>
        /// <returns></returns>
        public static Message_ClientRequest InternalDeserialize(byte[] bytes) => new Message_ClientRequest((Request)bytes[1], bytes[2]);



        /// <summary>
        /// Enum holding the valid types of request.
        /// </summary>
        public enum Request : byte
        {
            /// <summary>
            /// Default value to prevent the reading of invalid data.
            /// </summary>
            None = 0,
            /// <summary>
            /// Request new batches for execution.
            /// </summary>
            NewBatch,
            /// <summary>
            /// Request the returning of a completed batch.
            /// </summary>
            ReturnBatch,
        }
    }
}
