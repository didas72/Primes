using System;

namespace BatchServer.Messages
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
                case Type.Identifier:
                    return Message_Identifier.InternalDeserialize(bytes);

                case Type.Server_StateControl:
                    return new Message_Server_StateControl();

                case Type.Control_Control:
                    return Message_Control_Control.InternalDeserialize(bytes);

                case Type.Server_StateRequest:
                    return new Message_Server_StateRequest();

                case Type.Client_StateRequest:
                    return Message_Client_StateRequest.InternalDeserialize(bytes);

                default:
                    throw new Exception("Invalid message type.");
            }
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
        }
    }
}
