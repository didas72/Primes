using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

using DidasUtils.Data;
using DidasUtils.Logging;

namespace Primes.Common.Net
{
    public static class MessageBuilder
    {
        public static byte[] Message(string messageType, string target, byte[] value)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));
            if (messageType.Length != 3) throw new ArgumentException("MessageType must always be 3 characters long.");

            List<byte> ret = new();
            ret.AddRange(Encoding.UTF8.GetBytes(messageType));
            if (string.IsNullOrEmpty(target))
            {
                ret.AddRange(BitConverter.GetBytes((ushort)0));//no target
            }
            else
            {
                ret.AddRange(BitConverter.GetBytes((ushort)target.Length));
                ret.AddRange(Encoding.UTF8.GetBytes(target));
            }
            if (value == null || value.Length == 0)
            {
                ret.AddRange(BitConverter.GetBytes(0));
            }
            else
            {
                ret.AddRange(BitConverter.GetBytes(value.Length));
                ret.Add(0);//this case it is a byte array
                ret.AddRange(value);
            }

            return ret.ToArray();
        }
        public static byte[] Message(string messageType, string target, string value)
        {
            if (messageType == null) throw new ArgumentNullException(nameof(messageType));
            if (messageType.Length != 3) throw new ArgumentException("MessageType must always be 3 characters long.");

            List<byte> ret = new();
            ret.AddRange(Encoding.UTF8.GetBytes(messageType));
            if (string.IsNullOrEmpty(target))
            {
                ret.AddRange(BitConverter.GetBytes((ushort)0));//no target
            }
            else
            {
                ret.AddRange(BitConverter.GetBytes((ushort)target.Length));
                ret.AddRange(Encoding.UTF8.GetBytes(target));
            }
            if (string.IsNullOrEmpty(value))
            {
                ret.AddRange(BitConverter.GetBytes(0));
            }
            else
            {
                ret.AddRange(BitConverter.GetBytes(value.Length));
                ret.Add(1);//this case it is a string
                ret.AddRange(Encoding.UTF8.GetBytes(value));
            }

            return ret.ToArray();
        }



        public static byte[] Ping() => Message("png", string.Empty, string.Empty);



        public static byte[] ResponseActionSuccess(string comment = "")
        {
            List<byte> response = new();
            response.AddRange(Encoding.UTF8.GetBytes("ret"));
            response.AddRange(BitConverter.GetBytes((ushort)0));//no target
            response.AddRange(BitConverter.GetBytes(comment.Length + 5));//comment + 'PASS:'
            response.Add(1);//string
            response.AddRange(Encoding.UTF8.GetBytes("PASS:"));
            if (!string.IsNullOrEmpty(comment))
                response.AddRange(Encoding.UTF8.GetBytes(comment));

            return response.ToArray();
        }
        public static byte[] ResponseActionFail(string comment = "")
        {
            List<byte> response = new();
            response.AddRange(Encoding.UTF8.GetBytes("ret"));
            response.AddRange(BitConverter.GetBytes((ushort)0));//no target
            response.AddRange(BitConverter.GetBytes(comment.Length + 5));//comment + 'FAIL:'
            response.Add(1);//string
            response.AddRange(Encoding.UTF8.GetBytes("FAIL:"));
            if (!string.IsNullOrEmpty(comment))
                response.AddRange(Encoding.UTF8.GetBytes(comment));

            return response.ToArray();
        }
        public static byte[] ResponseActionInvalid(string comment = "")
        {
            List<byte> response = new();
            response.AddRange(Encoding.UTF8.GetBytes("ret"));
            response.AddRange(BitConverter.GetBytes((ushort)0));//no target
            response.AddRange(BitConverter.GetBytes(comment.Length + 5));//comment + 'NVAL:'
            response.Add(1);//string
            response.AddRange(Encoding.UTF8.GetBytes("NVAL:"));
            if (!string.IsNullOrEmpty(comment))
                response.AddRange(Encoding.UTF8.GetBytes(comment));

            return response.ToArray();
        }



        public static bool ReceiveMessage(NetworkStream ns, out byte[] message, TimeSpan timeout = new TimeSpan())
        {
            message = SegmentedData.ReadFromSocket(ns, 4096, timeout);
            return message.Length != 0;
        }
        public static void DeserializeMessage(byte[] msg, out string messageType, out string target, out object value)
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            Log.LogEvent($"Deserializing message len:{msg.Length}", "DeserializeMessage");

            int head = 0;
            messageType = Encoding.UTF8.GetString(msg, head, 3); head += 3;
            short targetLen = BitConverter.ToInt16(msg, head); head += 2;
            target = targetLen == 0 ? string.Empty : Encoding.UTF8.GetString(msg, 5, targetLen); head += targetLen;
            int valueLength = BitConverter.ToInt32(msg, head);
            byte valueType; value = null;

            if (valueLength != 0)
            {
                valueType = msg[head++];

                switch (valueType)
                {
                    case 0:
                        value = new byte[msg.Length - head];
                        Array.Copy(msg, head, (byte[])value, 0, msg.Length - head);
                        break;

                    case 1:
                        value = Encoding.UTF8.GetString(msg, head, msg.Length - head);
                        break;
                }
            }
        }



        public static void SendMessage(byte[] message, NetworkStream ns)
        {
            SegmentedData.SendToStream(message, ns, 4096);
        }
    }
}
