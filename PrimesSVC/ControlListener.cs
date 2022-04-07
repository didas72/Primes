using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;

using DidasUtils.Logging;

using Primes.Common.Net;

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



        public static void ListenAndJoin()
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
                    Log.LogEvent($"Client connected at {socket.Client.RemoteEndPoint}", "ListenLoop");

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
            try
            {
                NetworkStream ns = client.GetStream();

                if (!MessageBuilder.ReceiveMessage(ns, out byte[] msg, new TimeSpan(0, 0, 0, 0, 300)))
                {
                    Log.LogEvent("Timed out while trying to receive message.", "HandleClient");
                    client.Close();
                    return;
                }
                MessageBuilder.DeserializeMessage(msg, out string messageType, out string target, out object value);
                if (!HandleMessage(messageType, target, value, out byte[] response))
                {
                    Log.LogEvent(Log.EventType.Warning, "Something went wrong while handling a client.", "HandleClient");
                    client.Close();
                    return;
                }

                MessageBuilder.SendMessage(response, ns);
                ns.Close();
                client.Close();

                Log.LogEvent("Handle complete", "ListenLoop");
            }
            catch (Exception e)
            {
                Log.LogException("Failed to handle client.", "HandleClient", e);
            }
            finally
            {
                client.Close();
            }
        }
        private static bool HandleMessage(string msgType, string target, object value, out byte[] response)
        {
            response = Array.Empty<byte>();

            switch (msgType)
            {
                case "run":
                    if (!MessageBuilder.ValidateActionMessage(msgType, target, value)) return false;
                    return HandleRunMessage((string)value, out response);

                case "png":
                    if (!MessageBuilder.ValidatePingMessage(msgType, target, value)) return false;
                    return HandlePingMessage(out response);

                case "req":
                    if (!MessageBuilder.ValidateRequestMessage(msgType, target, value)) return false;
                    return HandleRequestMessage((string)value, out response);

                case "ret":
                case "set":
                case "dta":
                    throw new NotImplementedException();

                default:
                    return false;
            }
        }



        private static bool HandleRunMessage(string value, out byte[] response)
        {
            switch (value)
            {
                case "start":
                    WorkCoordinator.StartWork();
                    response = MessageBuilder.ResponseActionSuccess();
                    return true;

                case "stop":
                    WorkCoordinator.StopWork();
                    response = MessageBuilder.ResponseActionSuccess();
                    return true;

                case "fstop":
                    WorkCoordinator.StopWork();
                    response = MessageBuilder.ResponseActionSuccess();
                    doListen = false;
                    return true;

                default:
                    response = MessageBuilder.ResponseActionInvalid();
                    Log.LogEvent(Log.EventType.Warning, $"Received invalid or unhandled action '{value}'.", "HandleRunMessage");
                    return true;
            }
        }
        private static bool HandlePingMessage(out byte[] response)
        {
            response = MessageBuilder.ResponseActionSuccess();
            return true;
        }
        private static bool HandleRequestMessage(string value, out byte[] response)
        {
            switch (value)
            {
                case "rstatus":
                    response = MessageBuilder.ResponseRequestSuccess(WorkCoordinator.IsWorkRunning().ToString());
                    return true;

                case "cbnum":
                    response = MessageBuilder.ResponseRequestSuccess(WorkCoordinator.GetCurrentBatchNumber().ToString());
                    return true;

                case "cbprog":
                    response = MessageBuilder.ResponseRequestSuccess(WorkCoordinator.GetCurrentBatchProgress().ToString());
                    return true;

                default:
                    response = MessageBuilder.ResponseRequestInvalid();
                    Log.LogEvent(Log.EventType.Warning, $"Received invalid or unhandled request '{value}'.", "HandleRequestMessage");
                    return true;
            }
        }
    }
}
