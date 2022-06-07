using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;

using DidasUtils.Logging;
using DidasUtils.Net;

using Primes.Common.Net;

namespace Primes.SVC
{
    internal static class BatchManager
    {
        private static bool initialized = false;
        private static string serverIP = string.Empty;
        private static ushort serverPort = 0;
        private static string homeDir;

        public static bool Init()
        {
            try
            {
                string filePath = Path.Combine(homeDir = Settings.HomeDir, "serverCfg.cfg");

                string[] lines = File.ReadAllLines(filePath);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    lines[i] = lines[i].Trim();
                    if (lines[i].StartsWith("//")) continue;

                    if (lines[i].StartsWith("serverIp="))
                        serverIP = lines[i][9..];
                    else if (lines[i].StartsWith("serverPort="))
                        serverPort = ushort.Parse(lines[i][11..]);
                    else continue;
                }

                if (serverPort < 1024) throw new Exception("Port was not set.");
                if (string.IsNullOrEmpty(serverIP)) throw new Exception("IP was not set.");
            } 
            catch (Exception e)
            {
                Log.LogException($"Failed to load serverCfg.", "BatchRetriever", e);
                return false;
            }

            return initialized = true;
        }



        public static bool IsServerAccessible()
        {
            if (!initialized) throw new Exception("Attempted to use uninitialized BatchRetriever.");
            bool ret;

            try
            {
                TcpClient cli = new(serverIP, serverPort);
                ret = cli.Connected;
                cli.Close();

                return ret;
            }
            catch (Exception e)
            {
                Log.LogException("Failed to check whether server is accessible.", "BatchRetriever", e);
                return false;
            }
        }
        public static bool GetBatches(TimeSpan timeout)
        {
            //TODO: More returned info?

            Log.LogEvent($"Attempting to get batches from '{serverIP}:{serverPort}'.", "GetBatches");

            try
            {
                TcpClient cli = new(serverIP, serverPort);
                if (!cli.Connected) throw new Exception("Failed to connect to server.");
                Socket soc = cli.Client;
                NetworkStream ns = cli.GetStream();

                if (!MessageBuilder.ReceiveMessage(ns, out byte[] msg, timeout)) throw new Exception("Failed to get intent request message.");
                MessageBuilder.DeserializeMessage(msg, out string msgType, out string tgt, out object value);
                if (!MessageBuilder.ValidateRequestMessage(msgType, tgt, value)) throw new Exception("Recieved unexpected message.");
                if ((string)value != "intent") throw new Exception("Recieved unexpected message.");

                MessageBuilder.SendMessage(MessageBuilder.Message("ret", string.Empty, "get"), ns);

                if (!MessageBuilder.ReceiveMessage(ns, out msg, timeout)) throw new Exception("Failed to get server response.");
                MessageBuilder.DeserializeMessage(msg, out msgType, out tgt, out value);
                if (MessageBuilder.ValidateErrorMessage(msgType, tgt, value))
                {
                    cli.Close();
                    return false;
                }
                else if (MessageBuilder.ValidateDataMessage(msgType, tgt, value))
                {
                    //TODO:
                }
                else throw new Exception("Received unexpected message.");

                //TODO:

                throw new NotImplementedException();

                ns.Close();
                cli.Close();
            }
            catch (Exception e)
            {
                Log.LogException("Failed to get batches.", "BatchRetriever", e);
                return false;
            }
        }
    }
}
