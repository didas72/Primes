using System;

namespace BatchServer.Messages
{
    public class Message_Server_StateRequest : Message
    {
        public Message_Server_StateRequest()
        {
            MessageType = Type.Server_StateRequest;
        }



        public override byte[] Serialize()
        {
            return new byte[1] { (byte)MessageType };
        }
    }
}
