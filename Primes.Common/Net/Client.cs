using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Primes.Common;
using Primes.Common.Net;
using Primes.Common.Net.Messages;

namespace Primes.Common.Net
{
    /// <summary>
    /// Wrapper class that contains a TcpClient and useful networking methods.
    /// </summary>
    public class Client
    {
        /// <summary>
        /// The wrapped TcpClient instance.
        /// </summary>
        public TcpClient socket;
        /// <summary>
        /// The callback for when a message is received.
        /// </summary>
        public MessageReceived messageReceived;

        private readonly NetworkStream netStream;
        private Thread receiveThread;
        private volatile bool doListen = false;

        private const ushort blockSize = ushort.MaxValue;



        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public delegate void MessageReceived(IMessage message);



        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="socket">The TcpClient to wrap.</param>
        public Client(TcpClient socket)
        {
            this.socket = socket;
            socket.SendBufferSize = blockSize;
            socket.ReceiveBufferSize = blockSize;
            netStream = socket.GetStream();
        }



        /// <summary>
        /// Starts listening for incoming messages, calling <see cref="messageReceived"/> callback when needed.
        /// </summary>
        public void StartListening()
        {
            doListen = true;
            receiveThread = new Thread(ReceiveLoop);
            receiveThread.Start();
        }
        /// <summary>
        /// Stops listening for incoming messages.
        /// </summary>
        public void StopListening()
        {
            doListen = false;
            receiveThread.Join();
        }



        /// <summary>
        /// Sends an <see cref="IMessage"/> thorugh the TcpClient.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        /// <returns>Boolean indicating the operation's success.</returns>
        public bool SendMessage(IMessage message)
        {
            try
            {
                byte[] buffer = message.Serialize();

                if (socket.Connected)
                {
                    SegmentedData.SendToStream(buffer, netStream, blockSize);
                }
                else
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }
        /// <summary>
        /// Disconnects the TcpClient and disposes all resources.
        /// </summary>
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
                        byte[] buffer = SegmentedData.ReadFromStream(netStream, blockSize);

                        IMessage message = Message.Deserialize(buffer);

                        messageReceived.BeginInvoke(message, null, this);
                    }
                }
                catch { }

                Thread.Sleep(10);
            }
        }
    }
}
