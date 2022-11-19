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

                while (doListen)
                {
                    TcpClient socket = listener.AcceptTcpClient();
                    Log.LogEvent($"Client connected at {socket.Client.RemoteEndPoint}", "ListenLoop");

                    //only handle one at a time
                    HandleClient(socket);
                }

                listener.Stop();
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
                    Log.LogEvent(Log.EventType.Warning, "Timed out while trying to receive message.", "HandleClient");
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

                string debugValue = value is string s ? s : "object";

                Log.LogEvent($"Handle complete. type='{messageType}';target='{target}';value='{debugValue}'", "ListenLoop");
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

            //TODO: Handle freezes
            //TODO: Start stop return as failed to start stop
            //TODO: More logging options
            //TODO: Here/PrimesUI, premature / unhandled disconnections

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
                    Log.LogEvent(Log.EventType.Warning, $"Invalid msgType '{msgType}'.", "HandleMessage");
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
                    Log.LogEvent($"Started work, status: {WorkCoordinator.IsWorkRunning()}.", "HandleRunMessage");
                    return true;

                case "stop":
                    WorkCoordinator.StopWork();
                    response = MessageBuilder.ResponseActionSuccess();
                    Log.LogEvent($"Stopped work, status: {!WorkCoordinator.IsWorkRunning()}.", "HandleRunMessage");
                    return true;

                case "trun":
                    if (WorkCoordinator.IsWorkRunning())
                    { WorkCoordinator.StopWork(); Log.LogEvent($"T-Stopped work, status: {!WorkCoordinator.IsWorkRunning()}.", "HandleRunMessage"); }
                    else
                    { WorkCoordinator.StartWork(); Log.LogEvent($"T-Started work, status: {WorkCoordinator.IsWorkRunning()}.", "HandleRunMessage"); }
                    response = MessageBuilder.ResponseActionSuccess();
                    return true;

                case "fstop":
                    response = MessageBuilder.ResponseActionSuccess();
                    doListen = false;
                    return true;

                default:
                    response = MessageBuilder.ResponseActionInvalid();
                    Log.LogEvent(Log.EventType.Warning, $"Received invalid or unhandled action '{value}'.", "HandleRunMessage");
                    return false;
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
                case "rstatus": //run status
                    response = MessageBuilder.ResponseRequestSuccess(WorkCoordinator.IsWorkRunning() ? "Running." : "Stopped.");
                    Log.LogEvent("Response is " + (WorkCoordinator.IsWorkRunning() ? "Running." : "Stopped."), "HandleRequestMessage");
                    return true;

                case "cbnum": //batch num
                    response = MessageBuilder.ResponseRequestSuccess(WorkCoordinator.GetCurrentBatchNumber().ToString());
                    Log.LogEvent("Response is " + WorkCoordinator.GetCurrentBatchNumber().ToString(), "HandleRequestMessage");
                    return true;

                case "cbprog": //batch progress
                    string prog = WorkCoordinator.GetCurrentBatchProgress().ToString();
                    response = MessageBuilder.ResponseRequestSuccess(prog);
                    Log.LogEvent("Response is " + prog, "HandleRequestMessage");
                    return true;

                case "reslen": //resource length
                    try
                    {
                        long len = new FileInfo(Path.Combine(Globals.resourcesDir, "knownPrimes.rsrc")).Length;
                        response = MessageBuilder.ResponseRequestSuccess("0x"+len.ToString("X2"));
                        return true;
                    }
                    catch
                    {
                        response = MessageBuilder.ResponseRequestInvalid("Failed to get resource length.");
                        return true;
                    }
                    

                default:
                    response = MessageBuilder.ResponseRequestInvalid();
                    Log.LogEvent(Log.EventType.Warning, $"Received invalid or unhandled request '{value}'.", "HandleRequestMessage");
                    return false;
            }
        }
    }
}
