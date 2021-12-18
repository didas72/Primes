using System;

namespace BatchServer.Messages
{
    public class Message_Server_Confirm : Message
    {
        public Message_Server_Confirm()
        {
            MessageType = Type.Server_Confirm;
        }

        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType };
        }
    }
}
