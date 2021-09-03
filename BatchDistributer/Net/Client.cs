using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Primes.Common;
using Primes.Common.Net;
using Primes.Common.Net.Messages;

namespace Primes.BatchDistributer.Net
{
    public class Client
    {
        public TcpClient socket;
        private NetworkStream netStream;
        private Thread receiveThread;
        private volatile bool doListen = false;

        public MessageReceived messageReceived;



        public delegate void MessageReceived(IMessage message);



        public Client() { }
        public Client(TcpClient socket)
        {
            this.socket = socket;
            netStream = socket.GetStream();
        }



        public void StartListening()
        {
            doListen = true;
            receiveThread = new Thread(ReceiveLoop);
            receiveThread.Start();
        }
        public void StopListening()
        {
            doListen = false;
            receiveThread.Join();
        }



        public bool SendMessage(IMessage message)
        {
            Log.LogEvent($"Sending message of type {message.MessageType} to client {socket.Client.RemoteEndPoint}.", "Client");

            try
            {
                byte[] bytes = message.Serialize();
                byte[] buffer = new byte[bytes.Length + 4];

                Array.Copy(BitConverter.GetBytes(bytes.Length - 4), 0, buffer, 0, 4);
                Array.Copy(bytes, 0, buffer, 4, bytes.Length);

                if (socket.Connected)
                    netStream.Write(buffer, 0, buffer.Length);
                else
                    return false;
            }
            catch (Exception e)
            {
                Log.LogEvent(Log.EventType.Error, $"Failed to send message of type {message.MessageType} to client {socket.Client.RemoteEndPoint}: {e.Message}.", "Client");
                return false;
            }

            return true;
        }
        public void Disconnect()
        {
            StopListening();
            netStream.Dispose();
            socket.Close();
        }



        private void ReceiveLoop()
        {
            while (doListen)
            {
                try
                {
                    if (netStream.DataAvailable)
                    {
                        byte[] buffer = new byte[4];

                        netStream.Read(buffer, 0, 4);

                        int size = BitConverter.ToInt32(buffer, 0);

                        int len = 0, head = 0;
                        buffer = new byte[len];

                        while (len < size)
                        {
                            netStream.Read(buffer, head, Math.Min(len, socket.ReceiveBufferSize));
                        }

                        IMessage message = Message.Deserialize(buffer);

                        Log.LogEvent($"Received message of type {message.MessageType}.", "ClientReceiveThread");

                        messageReceived.BeginInvoke(message, null, this);
                    }
                }
                catch { }

                Thread.Sleep(10);
            }
        }
    }
}
