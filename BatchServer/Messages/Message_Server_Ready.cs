using System;

namespace BatchServer.Messages
{
    public class Message_Server_Ready : Message
    {
        public Message_Server_Ready()
        {
            MessageType = Type.Server_Ready;
        }

        public override byte[] Serialize()
        {
            return new byte[] { (byte)MessageType };
        }
    }
}
