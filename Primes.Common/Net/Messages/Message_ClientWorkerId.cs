using System;
using System.Text;

namespace Primes.Common.Net.Messages
{
    /// <summary>
    /// Message used to inform the server what workerId the client has in storage.
    /// </summary>
    public class Message_ClientWorkerId : Message
    {
        /// <summary>
        /// The workerId present in the client's storage.
        /// </summary>
        public string workerId;



        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="workerId">The workerId present in the client's storage.</param>
        public Message_ClientWorkerId(string workerId)
        {
            MessageType = Type.Client_WorkerId;
            this.workerId = workerId;
        }



        /// <summary>
        /// Serializes the given message to a byte array.
        /// </summary>
        /// <returns></returns>
        public override byte[] Serialize()
        {
            byte[] bytes = new byte[5];
            bytes[0] = (byte)MessageType;
            Array.Copy(Encoding.ASCII.GetBytes(workerId), 0, bytes, 1, 4);
            return bytes;
        }



        /// <summary>
        /// Deserializes a message from a byte array.
        /// </summary>
        /// <param name="bytes">The byte array containig the message to deserialize.</param>
        /// <returns></returns>
        public static Message_ClientWorkerId InternalDeserialize(byte[] bytes) => new Message_ClientWorkerId(Encoding.ASCII.GetString(bytes, 1, 4));
    }
}
