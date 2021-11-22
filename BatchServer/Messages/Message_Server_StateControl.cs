using System;

namespace BatchServer.Messages
{
    public class Message_Server_StateControl : Message
    {
        public Message_Server_StateControl()
        {
            MessageType = Type.Server_StateControl;
        }



        public override byte[] Serialize()
        {
            return new byte[1] { (byte)MessageType };
        }
    }
}
