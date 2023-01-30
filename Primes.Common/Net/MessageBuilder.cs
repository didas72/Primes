using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net.Sockets;

using DidasUtils.Data;
using DidasUtils.Logging;
using DidasUtils.ErrorCorrection;

namespace Primes.Common.Net
{
    public static class MessageBuilder
    {
        private const int blockSize = 4096;


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
                int lent = value.Length;
                ret.AddRange(BitConverter.GetBytes(lent));
                ret.Add(1);//this case it is a string
                ret.AddRange(Encoding.UTF8.GetBytes(value));
            }

            return ret.ToArray();
        }



        public static byte[] Ping() => Message("png", string.Empty, string.Empty);



        public static byte[] ResponseActionSuccess(string comment = "") => Message("ret", string.Empty, "ACTION_PASS:" + comment);
        public static byte[] ResponseActionFail(string comment = "") => Message("ret", string.Empty, "ACTION_FAIL:" + comment);
        public static byte[] ResponseActionInvalid(string comment = "") => Message("ret", string.Empty, "ACTION_NVAL:" + comment);


        public static byte[] ResponseRequestSuccess(string comment = "") => Message("ret", string.Empty, "REQUEST_PASS:" + comment);
        public static byte[] ResponseRequestInvalid(string comment = "") => Message("ret", string.Empty, "REQUEST_NVAL:" + comment);


        public static bool ValidateReturnMessage(string messageType, string target, object value)
        {
            if (string.IsNullOrEmpty(messageType) || messageType != "ret" || !string.IsNullOrEmpty(target) || value is not string || string.IsNullOrEmpty((string)value)) return false;
            return true;
        }
        public static bool ValidateActionMessage(string messageType, string target, object value)
        {
            if (string.IsNullOrEmpty(messageType) || messageType != "run" || !string.IsNullOrEmpty(target) || value is not string || string.IsNullOrEmpty((string)value)) return false;
            return true;
        }
        public static bool ValidateRequestMessage(string messageType, string target, object value)
        {
            if (string.IsNullOrEmpty(messageType) || messageType != "req" || !string.IsNullOrEmpty(target) || value is not string || string.IsNullOrEmpty((string)value)) return false;
            return true;
        }
        public static bool ValidatePingMessage(string messageType, string target, object value)
        {
            if (string.IsNullOrEmpty(messageType) || messageType != "png" || !string.IsNullOrEmpty(target) || value != null) return false;
            return true;
        }
        public static bool ValidateErrorMessage(string messageType, string target, object value)
        {
            if (string.IsNullOrEmpty(messageType) || messageType != "err" || !string.IsNullOrEmpty(target) || value is not string || string.IsNullOrEmpty((string)value)) return false;
            return true;
        }
        public static bool ValidateAckMessage(string messageType, string target, object value)
        {
            if (string.IsNullOrEmpty(messageType) || messageType != "ack" || !string.IsNullOrEmpty(target) || value != null) return false;
            return true;
        }
        public static bool ValidateDataMessage(string messageType, string target, object value)
        {
            if (string.IsNullOrEmpty(messageType) || messageType != "dta" || !string.IsNullOrEmpty(target) || value == null) return false;
            return true;
        }



        public static bool ReceiveMessage(NetworkStream ns, out byte[] message, TimeSpan timeout = new TimeSpan())
        {
            message = SegmentedData.ReadFromSocket(ns, 4096, timeout);
            return message.Length != 0;
        }
        public static void DeserializeMessage(byte[] msg, out string messageType, out string target, out object value)
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            int head = 0;
            messageType = Encoding.UTF8.GetString(msg, head, 3); head += 3;
            short targetLen = BitConverter.ToInt16(msg, head); head += 2;
            target = targetLen == 0 ? string.Empty : Encoding.UTF8.GetString(msg, 5, targetLen); head += targetLen;
            int valueLength = BitConverter.ToInt32(msg, head); head += 4;
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
                        value = valueLength == 0 ? null : Encoding.UTF8.GetString(msg, head, msg.Length - head);
                        break;
                }
            }

            return;
        }



        public static void SendMessage(byte[] message, NetworkStream ns)
        {
            SegmentedData.SendToStream(message, ns, 4096);
        }
        public static void SendMessage(byte[] message, TcpClient cli)
        {
            SegmentedData.SendToStream(message, cli.GetStream(), 4096);
        }




        /* Segmented data
         * Messages sent:
         * 1) Header
         *      -Total data length (-1 if unknown) (int)
         *      -Max block size (int)
         * 2) Block(s)
         *      -Block number (not needed for Tcp but sanity check) (int)
         *      -Data size (last will be smaller, could be computed but safer) (int)
         *      -Data
         *      -(Padding (with 0s) if needed)
         *      -Fletcher32 check (data only) (not needed for Tcp but sanity check) (int)
         * 3) Tail
         *      -Block number = -1 (int)
        */
        public static bool SendStreamData(Stream source, NetworkStream ns, int timeoutMillis)
        {
            int prevWriteMillis = ns.WriteTimeout;
            ns.WriteTimeout = timeoutMillis;

            try
            {
                //send header
                byte[] buffer = new byte[8];
                int length = -1;

                try { length = (int)(source.Length - source.Position); } catch { }

                Array.Copy(BitConverter.GetBytes(length), 0, buffer, 0, 4);
                Array.Copy(BitConverter.GetBytes(blockSize), 0, buffer, 4, 4);

                ns.Write(buffer, 0, buffer.Length);//send



                //send blocks
                buffer = new byte[blockSize]; byte[] data = new byte[blockSize - 12]; int read, blockCount = 0; uint fletcher32;

                do
                {
                    read = source.Read(data, 0, data.Length); if (read == 0) break;
                    if (read != blockSize - 12)//padding
                    {
                        for (int i = read; i < data.Length; i++) data[i] = 0;
                    }
                    fletcher32 = Fletcher.Fletcher32(data);

                    Array.Copy(BitConverter.GetBytes(blockCount++), 0, buffer, 0, 4);
                    Array.Copy(BitConverter.GetBytes(read), 0, buffer, 4, 4);
                    Array.Copy(data, 0, buffer, 8, data.Length);
                    Array.Copy(BitConverter.GetBytes(fletcher32), 0, buffer, blockSize - 4, 4);

                    ns.Write(buffer, 0, buffer.Length);
                }
                while (read > 0);

                //send tail
                ns.Write(BitConverter.GetBytes((int)-1), 0, 4);

            }
            catch (Exception e)
            {
                Log.LogException("Failed to send data from stream.", "SendStreamData", e);
                try { ns.WriteTimeout = prevWriteMillis; } catch { }
                return false;
            }

            ns.WriteTimeout = prevWriteMillis;
            return true;
        }
        public static bool ReceiveStreamData(Stream output, NetworkStream ns, int timeoutMillis) 
        {
            int prevReadMillis = ns.ReadTimeout;
            ns.ReadTimeout = timeoutMillis;

            try
            {
                //read header
                byte[] buffer = new byte[8];
                int read = ns.Read(buffer, 0, buffer.Length); if (read != 8) throw new Exception("Could not read entire header from socket.");

                int totalDataLength = BitConverter.ToInt32(buffer, 0); //used if outputting to an array instead of a stream for example (pre alloc mem)
                int blockSize = BitConverter.ToInt32(buffer, 4); if (blockSize <= 12) throw new Exception("Invalid header.");


                //read blocks/tail
                buffer = new byte[blockSize]; byte[] data = new byte[blockSize - 12];
                uint fletcherReceived, fletcherCalculated; int blockNum, receivedData = 0, dataSize, lastBlock = -1; //-1 to syncronize everything right off the bat

                do
                {
                    read = ns.Read(buffer, 0, buffer.Length);

                    blockNum = BitConverter.ToInt32(buffer, 0);

                    if (read == blockSize) //block
                    {
                        if (blockNum != ++lastBlock) throw new Exception("Could not receive all blocks.");

                        dataSize = BitConverter.ToInt32(buffer, 4); if (dataSize > blockSize - 12 || dataSize <= 0) throw new Exception("Invalid block header.");
                        Array.Copy(buffer, 8, data, 0, blockSize - 12);
                        fletcherReceived = BitConverter.ToUInt32(buffer, blockSize - 4); fletcherCalculated = Fletcher.Fletcher32(data);
                        if (fletcherReceived != fletcherCalculated) throw new Exception($"Data corrupted. ({fletcherReceived}r=/={fletcherCalculated}c)");

                        receivedData += dataSize;
                        output.Write(buffer, 8, dataSize); output.Flush();
                    }
                    else if (read == 4) //probably tail
                    {
                        if (blockNum == -1) break; //it is tail
                        else throw new Exception("Could not read entire block/tail from socket.");
                    }
                    else throw new Exception("Could not read entire block/tail from socket.");
                }
                while (receivedData < totalDataLength);
            }
            catch (Exception e)
            {
                Log.LogException("Failed to receive data to stream.", "ReceiveStreamData", e);
                try { ns.WriteTimeout = prevReadMillis; } catch { }
                return false;
            }

            ns.ReadTimeout = prevReadMillis;
            return true;
        }
    }
}
