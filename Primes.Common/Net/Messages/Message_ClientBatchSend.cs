using System;

namespace Primes.Common.Net.Messages
{
    /// <summary>
    /// Message used to tranfer completed batches from the client to the server.
    /// </summary>
    public class Message_ClientBatchSend : Message
    {
        /// <summary>
        /// The number of batches present in the message. Used for error checking.
        /// </summary>
        public byte objectCount;
        /// <summary>
        /// The actual batch data.
        /// </summary>
        public byte[] objectData;



        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="objectCount">The number of batches present in the message. Used for error checking.</param>
        /// <param name="objectData">The actual batch data.</param>
        public Message_ClientBatchSend(byte objectCount, byte[] objectData)
        {
            this.objectCount = objectCount;
            this.objectData = objectData;
            MessageType = Type.Client_Batch_Send;
        }
        private Message_ClientBatchSend() { }



        /// <summary>
        /// Serializes the given message to a byte array.
        /// </summary>
        /// <returns></returns>
        public override byte[] Serialize()
        {
            byte[] bytes = new byte[2 + objectData.Length]; //1 type + 1 object count + data

            bytes[0] = (byte)MessageType;
            bytes[1] = objectCount;
            Array.Copy(objectData, 0, bytes, 2, objectData.Length);

            return bytes;
        }



        /// <summary>
        /// Deserializes a message from a byte array.
        /// </summary>
        /// <param name="bytes">The byte array containig the message to deserialize.</param>
        /// <returns></returns>
        public static Message_ClientBatchSend InternalDeserialize(byte[] bytes)
        {
            Message_ClientBatchSend ret = new Message_ClientBatchSend();
            ret.objectCount = bytes[1];
            Array.Copy(bytes, 2, ret.objectData, 0, bytes.Length - 2);
            return ret;
        }
    }
}
