using System;
using System.Text;

namespace Primes.Common.Net.Messages.Old
{
    /// <summary>
    /// Message used to inform the client that it should state it's request and provide the workerId to be used.
    /// </summary>
    public class Message_ServerStateRequest : Message
    {
        /// <summary>
        /// The workerId to be used.
        /// </summary>
        public string workerId;



        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="workerId">The workerId to be used.</param>
        public Message_ServerStateRequest(string workerId)
        {
            MessageType = Type.Server_State_Request;
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
        public static Message_ServerStateRequest InternalDeserialize(byte[] bytes) => new Message_ServerStateRequest(Encoding.ASCII.GetString(bytes, 1, 4));
    }
}
