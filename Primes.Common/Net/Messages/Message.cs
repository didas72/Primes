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
            return (Type)bytes[0] switch
            {
                Type.Identifier => Message_Identifier.InternalDeserialize(bytes),
                Type.Server_StateControl => new Message_Server_StateControl(),
                Type.Control_Control => Message_Control_Control.InternalDeserialize(bytes),
                Type.Server_StateRequest => new Message_Server_StateRequest(),
                Type.Client_StateRequest => Message_Client_StateRequest.InternalDeserialize(bytes),
                Type.Server_Serve => Message_Server_Serve.InternalDeserialize(bytes),
                Type.Client_Acknowledge => Message_Client_Acknowledge.InternalDeserialize(bytes),
                Type.Server_Data => Message_Server_Data.InternalDeserialize(bytes),
                Type.Server_Ready => new Message_Server_Ready(),
                Type.Client_Data => Message_Client_Data.InternalDeserialize(bytes),
                Type.Server_Acknowledge => Message_Server_Acknowledge.InternalDeserialize(bytes),
                Type.Server_Abort => new Message_Server_Abort(),
                Type.Server_Confirm => new Message_Server_Confirm(),
                _ => throw new Exception("Invalid message type."),
            };
        }



        public enum Type : byte
        {
            None = 0,

            Identifier = 1,

            Server_StateControl = 2,
            Control_Control = 3,

            Server_StateRequest = 4,
            Client_StateRequest = 5,
            Server_Serve = 6,
            Client_Acknowledge = 7,
            Server_Data = 8,
            Server_Ready = 9,
            Client_Data = 10,
            Server_Acknowledge = 11,

            Server_Confirm = 12,
            Server_Abort = 13,
        }
    }
}
