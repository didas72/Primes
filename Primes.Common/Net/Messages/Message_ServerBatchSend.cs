using System;

namespace Primes.Common.Net.Messages
{
    /// <summary>
    /// Message used to tranfer pending batches from the server to the client.
    /// </summary>
    public class Message_ServerBatchSend : Message
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
        public Message_ServerBatchSend(byte objectCount, byte[] objectData)
        {
            this.objectCount = objectCount;
            this.objectData = objectData;
            MessageType = Type.Server_Batch_Send;
        }
        private Message_ServerBatchSend() { }



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
        public static Message_ServerBatchSend InternalDeserialize(byte[] bytes)
        {
            Message_ServerBatchSend ret = new Message_ServerBatchSend();
            ret.objectCount = bytes[1];
            Array.Copy(bytes, 2, ret.objectData, 0, bytes.Length - 2);
            return ret;
        }
    }
}
