using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

using DidasUtils.Logging;

namespace Primes.SVC
{
    internal static class ControlListener
    {
        private static bool initialized = false;
        private static TcpListener listener;
        private static volatile bool doListen = false;



        public static bool Init()
        {
            listener = new TcpListener(Settings.AllowExternalControl ? IPAddress.Any : IPAddress.Loopback, Settings.ControlPort);

            initialized = true;
            return true;
        }



        public static void ListenAndMerge()
        {
            if (!initialized) throw new Exception("Attempted to start listening from an uninitialized ControlListener.");

            doListen = true;

            ListenLoop();
        }



        private static void ListenLoop()
        {
            try
            {
                listener.Start();

                while (true)
                {
                    if (!doListen)
                        break;

                    TcpClient socket = listener.AcceptTcpClient();

                    //only handle one at a time
                    HandleClient(socket);
                }
            }
            catch (Exception e)
            {
                Log.LogException("Error listening for controls.", "ControlListener", e);
            }
        }
        private static void HandleClient(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            List<byte> msg = new();
            byte[] buffer = new byte[1024];
            int attempts;
            bool handleDone = false;

            while (!handleDone)
            {
                attempts = 0; 

                while (attempts < 1000)
                {
                    if (!client.Connected)
                    {
                        ns.Dispose();
                        client.Close();
                    }

                    if (ns.Length > ns.Position) //has something to read
                    {
                        int trueRead = ns.Read(buffer, 0, buffer.Length);
                        msg.AddRange(buffer.AsSpan(0, trueRead).ToArray());

                        if (IsMessageComplete(msg)) break;
                    }
                    else
                    {
                        attempts++;
                        Thread.Sleep(1);
                    }
                }

                if (ProcessMessage(msg.Skip(4).ToArray(), out byte[] response);
                throw new NotImplementedException();
            }
        }
        private static bool IsMessageComplete(List<byte> ms)
        {
            if (ms.Count < 4) return false;

            int msgLen = BitConverter.ToInt32(ms.ToArray(), 0);
            return ms.Count - 4 == msgLen;
        }
        private static bool ProcessMessage(byte[] msg, out byte[] response)
        {
            int head = 0;
            string msgType = Encoding.UTF8.GetString(msg, head, 3); head += 3;
            short targetLen = BitConverter.ToInt16(msg, head); head += 2;
            string target = targetLen == 0 ? string.Empty : Encoding.UTF8.GetString(msg, 5, targetLen); head += targetLen;
            int valueLength = BitConverter.ToInt32(msg, head);
            byte valueType = 0; object value = null; response = Array.Empty<byte>();

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

            return HandleMessage(msgType, target, valueType, value, out response);
        }
        private static bool HandleMessage(string msgType, string target, byte valueType, object value, out byte[] response)
        {
            response = Array.Empty<byte>();

            switch (msgType)
            {
                default:
                    return false;
            }
        }
    }
}
