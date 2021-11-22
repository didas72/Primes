using System;
using System.Collections.Generic;

using DidasUtils.Net;

namespace BatchServer.Messages
{
    public class Message_Identifier : Message
    {
        public Identifier identifier;



        public Message_Identifier(Identifier id)
        {
            MessageType = Type.Identifier;
            identifier = id;
        }



        public override byte[] Serialize()
        {
            return new byte[2] { (byte)MessageType, (byte)identifier };
        }



        public static Message_Identifier InternalDeserialize(byte[] bytes)
        {
            if (bytes.Length != 2)
                throw new ArgumentException();

            return new Message_Identifier((Identifier)bytes[1]);
        }



        public enum Identifier : byte
        {
            None = 0,
            Client = 1,
            Control = 2,
        }
    }
}
