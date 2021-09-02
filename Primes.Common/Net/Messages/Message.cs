using System;

namespace Primes.Common.Net.Messages
{
    public interface IMessage
    {
        Message.Type MessageType { get; }
        byte[] Serialize();
    }

    public abstract class Message : IMessage
    {
        public Type MessageType { get; protected set; }



        public abstract byte[] Serialize();



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

                default:
                    throw new Exception("Invalid message type.");
            }
        }



        public enum Type : byte
        {
            None = 0,
            Server_Welcome_Wait,
            Server_Request_WorkerId,
            Client_WorkerId,
            Server_State_Request, //holds workerId, new one if needed
            Client_Request,

            //if client request asks for batch
            Server_Batch_Send,
            //if no batch available
            Server_Batch_Not_Available,

            //if client request asks to return batch
            Server_Batch_Return_Listening,
            Client_Batch_Send,

            //server closing connection
            Server_Close_Connection
        }
    }
}
