using DidasUtils.Net;

namespace BatchServer.Messages
{
    public static class ClientExtend
    {
        public static void SendMessage(this Client cl, Message msg) => cl.SendMessage(msg.Serialize());
    }
}
