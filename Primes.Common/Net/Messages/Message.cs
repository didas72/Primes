using System;

namespace Primes.Common.Net.Messages.Old
{
    /// <summary>
    /// Interface to hold minimum Methods and Properties a Message type class should implement.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// The type of message represented by the class.
        /// </summary>
        Message.Type MessageType { get; }
        /// <summary>
        /// Serializes the given message to a byte array.
        /// </summary>
        /// <returns></returns>
        byte[] Serialize();
    }



    /// <summary>
    /// Represents a base definition for the Message type classes as well as the generalised Deserialize method and the definition for message types.
    /// </summary>
    public abstract class Message : IMessage
    {
        /// <summary>
        /// The type of message represented by the class. 
        /// </summary>
        public Type MessageType { get; protected set; }



        /// <summary>
        /// Empty declaration of the function that serializes the given message to a byte array.
        /// </summary>
        /// <returns></returns>
        public abstract byte[] Serialize();



        /// <summary>
        /// Deserializes an IMessage from a byte array.
        /// </summary>
        /// <param name="bytes">The byte array containing the message to deserialize.</param>
        /// <returns></returns>
        public static Message Deserialize(byte[] bytes)
        {
            switch ((Type)bytes[0])
            {
                case Type.Server_Welcome_Wait:
                    return Message_ServerWelcomeWait.InternalDeserialize(bytes);

                case Type.Server_Request_WorkerId:
                    return Message_ServerRequestWorkerId.InternalDeserialize(bytes);

                case Type.Client_WorkerId:
                    return Message_ClientWorkerId.InternalDeserialize(bytes);

                case Type.Server_State_Request:
                    return Message_ServerStateRequest.InternalDeserialize(bytes);

                case Type.Client_Request:
                    return Message_ClientRequest.InternalDeserialize(bytes);

                case Type.Server_Batch_Send:
                    return Message_ServerBatchSend.InternalDeserialize(bytes);

                case Type.Server_Batch_Not_Available:
                    return Message_ServerBatchNotAvailable.InternalDeserialize(bytes);

                case Type.Server_Batch_Return_Listening:
                    return Message_ServerBatchReturnListening.InternalDeserialize(bytes);

                case Type.Client_Batch_Send:
                    return Message_ClientBatchSend.InternalDeserialize(bytes);

                case Type.Server_Close_Connection:
                    return Message_ServerCloseConnection.InternalDeserialize(bytes);

                case Type.Server_Failed_Transfer:
                    return Message_ServerFailedTransfer.InternalDeserialize(bytes);

                case Type.Client_Failed_Transfer:
                    return Message_ClientFailedTransfer.InternalDeserialize(bytes);

                default:
                    throw new Exception("Invalid message type.");
            }
        }



        /// <summary>
        /// Enum holding the valid types of message.
        /// </summary>
        public enum Type : byte
        {
            /// <summary>
            /// Default value to prevent the reading of invalid data.
            /// </summary>
            None = 0,
            /// <summary>
            /// First message exchanged in a conversation. Informs client to wait while the server handles other clients' requests.
            /// </summary>
            Server_Welcome_Wait,
            /// <summary>
            /// Server requests the client's workerId which may be used to decide on batch distribution.
            /// </summary>
            Server_Request_WorkerId,
            /// <summary>
            /// Client returns either a valid workerId or an empty one if no id is assigned to it.
            /// </summary>
            Client_WorkerId,
            /// <summary>
            /// Server asks client to state it's request to later serve it. Includes the workerId to be assigned to the client, regardless of the one it may already hold.
            /// </summary>
            Server_State_Request,
            /// <summary>
            /// Client states it's request.
            /// </summary>
            Client_Request,

            /// <summary>
            /// Server's answer to a client request of type 'new batch'.
            /// </summary>
            Server_Batch_Send,
            /// <summary>
            /// Client confirms receiving of new batches, informing server it is free to update it's database.
            /// </summary>
            Client_Batch_Received,
            /// <summary>
            /// Server may reply that it will not be serving any new batches, optionally providing a reason.
            /// </summary>
            Server_Batch_Not_Available,

            /// <summary>
            /// Server's answer to a client request of type 'return batch'. Informs client it is free to return the completed batch.
            /// </summary>
            Server_Batch_Return_Listening,
            /// <summary>
            /// Client sends the completed batch.
            /// </summary>
            Client_Batch_Send,
            /// <summary>
            /// Server confirms receiving of the completed batch, informing the client it is free to remove it from it's system.
            /// </summary>
            Server_Batch_Received,

            /// <summary>
            /// Server informs client the connection will be closed.
            /// </summary>
            Server_Close_Connection,

            /// <summary>
            /// Server may inform the client it received the batch but it is either corrupt, incomplete, rejected or invalid.
            /// </summary>
            Server_Failed_Transfer,

            /// <summary>
            /// Client may inform the server it receive the batches but they are either corrupted or incomplete.
            /// </summary>
            Client_Failed_Transfer,
        }
    }
}
