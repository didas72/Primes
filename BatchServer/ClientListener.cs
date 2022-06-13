using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using DidasUtils;
using DidasUtils.Logging;

using Primes.Common.Net;

namespace BatchServer
{
    internal class ClientListener
    {
        private volatile bool running = false;


        private TcpListener listener;
        private Thread thread;


        private const string unexpectedMsgErr = "Recieved unexpected message.";
        private const string failedConnErr = "Failed to connect to server.";



        public ClientListener(int port)
        {
            listener = new(IPAddress.Any, port);
            thread = new Thread(() => ClientListenLoop(TimeSpan.FromSeconds(1))); 
        }



        public void Start()
        {
            running = true;
            thread.Start();
        }
        public void Stop()
        {
            running = false;

            if (thread != null && thread.IsAlive)
                thread.Join(20000); //20 secs, otherwise get ignored
        }



        private void ClientListenLoop(TimeSpan timeout)
        {
            listener.Start();

            while (running)
            {
                while (!listener.Pending())
                {
                    if (!running) goto ClientListenLoop_end;
                    Thread.Sleep(50); //20 checks per second, might be a bit overkill but trying to reduce latency
                }

                TcpClient cli = listener.AcceptTcpClient();
                Log.LogEvent($"Client connected from '{((IPEndPoint)cli.Client.RemoteEndPoint).Address}:{((IPEndPoint)cli.Client.RemoteEndPoint).Address}'.", "ClientListenLoop");

                try
                {
                    //directly handle, no need to check for concurrent connections, they can retry later

                    NetworkStream ns = cli.GetStream();

                    MessageBuilder.SendMessage(MessageBuilder.Message("req", string.Empty, "intent"), ns); //get intent

                    if (!MessageBuilder.ReceiveMessage(ns, out byte[] msg, timeout)) throw new Exception("Failed to get intent return message.");
                    MessageBuilder.DeserializeMessage(msg, out string msgType, out string tgt, out object value);
                    if (!MessageBuilder.ValidateReturnMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);

                    string[] parts = ((string)value).Split(";");
                    if (parts.Length != 2) throw new Exception("Received an invalid message.");
                    uint clientId = uint.Parse(parts[1]);

                    switch (parts[0])
                    {
                        case "get":
                            HandleGet();
                            break;

                        case "ret":
                            HandleRet();
                            break;

                        case "reget":
                            HandleReget();
                            break;

                        default:
                            throw new Exception("Invalid request.");
                    }

                    ns.Close();
                    cli.Close();
                }
                catch (Exception e)
                {
                    Log.LogException("Failed to serve request.", "ClientListenLoop", e);
                    try { cli?.Close(); } catch { }
                }
            }

        ClientListenLoop_end:
            listener.Stop();
        }



        private void HandleGet()
        {
            //TODO: Implement handle get
        }
        private void HandleRet()
        {
            //TODO: Implement handle ret
        }
        private void HandleReget()
        {
            //TODO: Implement handle reget
        }
    }
}
