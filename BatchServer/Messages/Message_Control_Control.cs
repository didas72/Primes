using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchServer.Messages
{
    public class Message_Control_Control : Message
    {
        public Control control;



        public Message_Control_Control(Control cntl)
        {
            MessageType = Type.Control_Control;
            control = cntl;
        }



        public override byte[] Serialize()
        {
            return new byte[2] { (byte)MessageType, (byte)control };
        }



        public static Message_Control_Control InternalDeserialize(byte[] bytes)
        {
            if (bytes.Length != 2)
                throw new ArgumentException();

            return new Message_Control_Control((Control)bytes[1]);
        }



        public enum Control : byte
        {
            None = 0,
            StopServer = 1,
        }
    }
}
