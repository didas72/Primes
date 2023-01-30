using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

using DidasUtils;
using DidasUtils.Logging;

using Primes.Common.Net;

namespace BatchServer
{
    internal class ClientListener
    {
        private volatile bool running = false;


        private readonly TcpListener listener;
        private readonly Thread thread;


        private const string unexpectedMsgErr = "Recieved unexpected message.";
        private const string invalidMsgErr = "Received an invalid message.";
        private const string failedConnErr = "Failed to connect to server.";
        private const string noAckErr = "Did not receive ACK message.";



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
                    //directly handle, no need to check for concurrent connections, clients can retry later

                    NetworkStream ns = cli.GetStream();

                    MessageBuilder.SendMessage(MessageBuilder.Message("req", string.Empty, "intent"), ns); //get intent

                    if (!MessageBuilder.ReceiveMessage(ns, out byte[] msg, timeout)) throw new Exception("Failed to get intent return message.");
                    MessageBuilder.DeserializeMessage(msg, out string msgType, out string tgt, out object value);
                    if (!MessageBuilder.ValidateReturnMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);

                    string[] parts = ((string)value).Split(";");
                    if (parts.Length != 2) throw new Exception(invalidMsgErr);
                    uint clientId = uint.Parse(parts[1]);

                    switch (parts[0])
                    {
                        case "get":
                            HandleGet(ns, clientId);
                            break;

                        case "ret":
                            HandleRet(ns, clientId);
                            break;

                        case "reget":
                            HandleReget(ns, clientId);
                            break;

                        default:
                            throw new Exception($"Invalid request '{parts[0]}'.");
                    }

                    ns.Close();
                    cli.Close();

                    Globals.clientData.ApplyAllPending();
                }
                catch (Exception e)
                {
                    Log.LogException("Failed to serve request.", "ClientListenLoop", e);
                    try { cli?.Close(); } catch { }
                    Globals.clientData.DiscardAllPending();
                }
            }

        ClientListenLoop_end:
            listener.Stop();
        }



        private void HandleGet(NetworkStream ns, uint clientId)
        {
            uint batch;
            if (!Globals.clientData.ExistsClientId(clientId)) { 
                clientId = Globals.clientData.PeekNewClientId();
                Globals.clientData.AddPending(Globals.clientData.AddNewClient);
            }
            else if (Globals.clientData.BatchLimitReached(clientId)) MessageBuilder.SendMessage(MessageBuilder.Message("err", null, "LimitReached"), ns);
            if ((batch = Globals.clientData.FindFreeBatch()) == 0) MessageBuilder.SendMessage(MessageBuilder.Message("err", null, "NoAvailableBatches"), ns);

            Globals.clientData.AddPending(() => Globals.clientData.AssignBatch(clientId, batch));

            byte[] batchBytes = File.ReadAllBytes(Path.Combine(Globals.sourceDir, batch + ".7z"));
            MessageBuilder.SendMessage(MessageBuilder.Message("dta", null, batchBytes), ns);

            if (!MessageBuilder.ReceiveMessage(ns, out byte[] replyBytes)) throw new Exception(noAckErr);
            MessageBuilder.DeserializeMessage(replyBytes, out string replyType, out string replyTarget, out object replyValue);
            if (!MessageBuilder.ValidateAckMessage(replyType, replyTarget, replyValue)) throw new Exception(unexpectedMsgErr);
        }
        private void HandleRet(NetworkStream ns, uint clientId)
        {
            //TODO: Implement HandleRet
        }
        private void HandleReget(NetworkStream ns, uint clientId)
        {
            //TODO: Implement HandleReget
        }
    }
}
