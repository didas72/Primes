﻿using System;

namespace BatchServer.Messages
{
    public class Message_Server_Acknowledge : Message
    {
        public byte[] data;



        public Message_Server_Acknowledge(byte[] data)
        {
            MessageType = Type.Server_Acknowledge;
            this.data = data;
        }



        public override byte[] Serialize()
        {
            byte[] ret = new byte[data.Length + 1];

            ret[0] = (byte)MessageType;
            Array.Copy(data, 0, ret, 1, data.Length);

            return ret;
        }



        public static Message_Server_Acknowledge InternalDeserialize(byte[] bytes)
        {
            byte[] data = new byte[bytes.Length - 1];
            Array.Copy(bytes, 1, data, 0, data.Length);

            return new Message_Server_Acknowledge(data);
        }
    }
}