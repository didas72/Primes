using System;

namespace BatchServer.Messages
{
    public class Message_Client_StateRequest : Message
    {
        public int userId;
        public Request request;
        public int amount;



        public Message_Client_StateRequest(int id, Request req, int am)
        {
            MessageType = Type.Client_StateRequest;
            userId = id;
            request = req;
            amount = am;
        }



        public override byte[] Serialize()
        {
            byte[] ret =  new byte[10];

            ret[0] = (byte)MessageType;
            Array.Copy(BitConverter.GetBytes(userId), 0, ret, 1, 4);
            ret[5] = (byte)request;
            Array.Copy(BitConverter.GetBytes(amount), 0, ret, 6, 4);

            return ret;
        }



        public static Message_Client_StateRequest InternalDeserialize(byte[] bytes)
        {
            int id = BitConverter.ToInt32(bytes, 1);
            int am = BitConverter.ToInt32(bytes, 6);

            return new Message_Client_StateRequest(id, (Request)bytes[5], am);
        }



        public enum Request : byte
        {
            None = 0,
            RequestBatches = 1,
            ReturnBatches = 2,
            ResendBacthes = 3,
            RequestTimeExtend = 4,
        }
    }
}
