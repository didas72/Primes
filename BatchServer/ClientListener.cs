using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

using DidasUtils;
using DidasUtils.Logging;

using Primes.Common.Net;
using System.Security.Cryptography.X509Certificates;

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

            Console.WriteLine("Listener started"); //FIXME: Remove this

            while (running)
            {
                while (!listener.Pending())
                {
                    if (!running) goto ClientListenLoop_end;
                    Thread.Sleep(50); //20 checks per second, might be a bit overkill but trying to reduce latency
                }

                TcpClient cli = listener.AcceptTcpClient();
                Log.LogEvent($"Client connected from '{((IPEndPoint)cli.Client.RemoteEndPoint).Address}:{((IPEndPoint)cli.Client.RemoteEndPoint).Port}'.", "ClientListenLoop");

                try
                {
                    //Directly handle, no need to check for concurrent connections, clients can retry later
                    NetworkStream ns = cli.GetStream();

                    MessageBuilder.SendMessage(MessageBuilder.BuildRequestMessage("intent"), ns); //get intent

                    Console.WriteLine("Requested intent"); //FIXME: Remove this

                    if (!MessageBuilder.ReceiveMessage(ns, out byte[] msg, timeout)) throw new Exception("Failed to get intent return message.");
                    MessageBuilder.DeserializeMessage(msg, out string msgType, out string tgt, out object value);
                    if (!MessageBuilder.ValidateReturnMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);

                    Console.WriteLine("Valid return"); //FIXME: Remove this

                    string[] parts = ((string)value).Split(";");
                    if (parts.Length != 2) throw new Exception(invalidMsgErr);
                    uint clientId = uint.Parse(parts[1]);

                    switch (parts[0])
                    {
                        case "get":
                            HandleGet(ns, clientId, timeout);
                            break;

                        case "ret":
                            HandleRet(ns, clientId, timeout);
                            break;

                        case "reget":
                            HandleReget(ns, clientId, timeout);
                            break;

                        default:
                            throw new Exception($"Invalid request '{parts[0]}'.");
                    }

                    ns.Close();
                    cli.Close();

                    Globals.clientData.ApplyAllPending();
                    Log.LogEvent("Request served.", "ClientListenLoop");
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
            Console.WriteLine("Listener stopped"); //FIXME: Remove this
        }



        private void HandleGet(NetworkStream ns, uint clientId, TimeSpan timeout)
        {
            Console.WriteLine("Handling Get"); //FIXME: Remove this

            uint batch;
            if (!Globals.clientData.ExistsClientId(clientId)) {
                clientId = Globals.clientData.PeekNewClientId();
                Globals.clientData.AddPending(Globals.clientData.AddNewClient);
            }
            else if (Globals.clientData.BatchLimitReached(clientId))
            {
                MessageBuilder.SendMessage(MessageBuilder.BuildErrorMessage($"{clientId};LimitReached"), ns);
                return;
            }
            if ((batch = Globals.clientData.FindFreeBatch()) == 0)
            {
                MessageBuilder.SendMessage(MessageBuilder.BuildErrorMessage($"{clientId};NoAvailableBatches"), ns);
                return;
            }

            Globals.clientData.AddPending(() => Globals.clientData.AssignBatch(clientId, batch));

            Console.WriteLine("Sending batch"); //FIXME: Remove this

            byte[] batchBytes = File.ReadAllBytes(Path.Combine(Globals.sourceDir, batch + ".7z"));
            MessageBuilder.SendMessage(MessageBuilder.BuildDataMessage(batchBytes), ns);

            Console.WriteLine("Sent batch"); //FIXME: Remove this

            if (!MessageBuilder.ReceiveMessage(ns, out byte[] replyBytes, timeout)) throw new Exception(noAckErr);
            MessageBuilder.DeserializeMessage(replyBytes, out string replyType, out string replyTarget, out object replyValue);
            if (!MessageBuilder.ValidateAckMessage(replyType, replyTarget, replyValue)) throw new Exception(unexpectedMsgErr);

            Console.WriteLine("Valid ack"); //FIXME: Remove this
        }
        private void HandleRet(NetworkStream ns, uint clientId, TimeSpan timeout)
        {
            Console.WriteLine("Handling Ret"); //FIXME: Remove this

            if (!Globals.clientData.ExistsClientId(clientId))
            {
                clientId = Globals.clientData.PeekNewClientId();
                Globals.clientData.AddPending(Globals.clientData.AddNewClient);
            }

            MessageBuilder.SendMessage(MessageBuilder.BuildRequestMessage("batchNum"), ns); //get batchNum

            Console.WriteLine("Requesting batchNum"); //FIXME: Remove this

            if (!MessageBuilder.ReceiveMessage(ns, out byte[] msg, timeout)) throw new Exception("Failed to get batchNum return message.");
            MessageBuilder.DeserializeMessage(msg, out string msgType, out string tgt, out object value);
            if (!MessageBuilder.ValidateReturnMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);

            if (!Globals.clientData.BatchAssignedToClient(clientId, uint.Parse((string)value)))
            {
                MessageBuilder.SendMessage(MessageBuilder.Message("err", null, $"{clientId};BatchNotAssigned"), ns);
                return;
            }

            Console.WriteLine("Requesting batch " + (string)value); //FIXME: Remove this

            MessageBuilder.SendMessage(MessageBuilder.BuildRequestMessage((string)value), ns);

            Console.WriteLine("Receiving batch"); //FIXME: Remove this

            FileStream fs = File.OpenWrite(Path.Combine(Globals.cacheDir, (string)value + ".7z"));
            if (!MessageBuilder.ReceiveStreamData(fs, ns, timeout.Milliseconds))
            {
                fs.Close();
                File.Move(Path.Combine(Globals.cacheDir, (string)value + ".7z"),
                    Path.Combine(Globals.cacheDir, (string)value + ".7z.failed"));
                return;
            }
            fs.Close();

            Console.WriteLine("Received batch"); //FIXME: Remove this

            MessageBuilder.SendMessage(MessageBuilder.BuildAckMessage(), ns);

            Console.WriteLine("Valid ack"); //FIXME: Remove this
        }
        private void HandleReget(NetworkStream ns, uint clientId, TimeSpan timeout)
        {
            //TODO: Implement HandleReget

            throw new NotImplementedException();
        }
    }
}
