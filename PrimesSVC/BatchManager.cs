using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;

using DidasUtils.Logging;
using DidasUtils.Files;

using Primes.Common.Net;

namespace Primes.SVC
{
    internal static class BatchManager
    {
        private static bool initialized = false;
        private static string serverIP = string.Empty;
        private static ushort serverPort = 0;
        private static uint clientId = 0;

        private const string unexpectedMsgErr = "Recieved unexpected message.";
        private const string failedConnErr = "Failed to connect to server.";



        public static bool Init()
        {
            try
            {
                string filePath = Path.Combine(Globals.homeDir, "serverCfg.cfg");
                string idPath = Path.Combine(Globals.homeDir, "clientId.cfg");
                string[] lines;

                if (File.Exists(filePath))
                {
                    lines = File.ReadAllLines(filePath);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;
                        lines[i] = lines[i].Trim();
                        if (lines[i].StartsWith("//")) continue;

                        if (lines[i].StartsWith("serverIp="))
                            serverIP = lines[i][9..];
                        else if (lines[i].StartsWith("serverPort="))
                            if (!ushort.TryParse(lines[i][11..], out serverPort))
                                throw new Exception("Invalid port value syntax.");
                        else continue;
                    }

                    if (serverPort < 1024) throw new Exception("Invalid/no port was set.");
                    if (string.IsNullOrEmpty(serverIP)) throw new Exception("IP was not set.");
                }
                else
                {
                    serverPort = 13031;
                    serverIP = "127.0.0.1";
                    File.WriteAllText(filePath, "serverIp=127.0.0.1\nserverPort=13031");
                }

                if (File.Exists(idPath))
                {
                    lines = File.ReadAllLines(idPath);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (string.IsNullOrWhiteSpace(lines[i])) continue;
                        lines[i] = lines[i].Trim();
                        if (lines[i].StartsWith("//")) continue;

                        if (lines[i].StartsWith("clientId="))
                            if (!uint.TryParse(lines[i][9..], out clientId)) Log.LogEvent("Failed to get clientId.", "BatchManager");
                            else continue;
                    }
                }
            } 
            catch (Exception e)
            {
                Log.LogException($"Failed to load serverCfg.", "BatchManager", e);
                return false;
            }

            return initialized = true;
        }



        public static bool IsServerAccessible()
        {
            if (!initialized) throw new Exception("Attempted to use uninitialized BatchManager.");
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
                Log.LogException("Failed to check whether server is accessible.", "BatchManager", e);
                return false;
            }
        }
        public static GetBatchStatus GetBatch(TimeSpan timeout)
        {
            if (!initialized) throw new Exception("Attempted to use uninitialized BatchManager.");

            Log.LogEvent($"Attempting to get batches from '{serverIP}:{serverPort}'.", "GetBatch");

            TcpClient cli = new();

            try
            {
                cli.Connect(serverIP, serverPort);
                if (!cli.Connected) throw new Exception(failedConnErr);
                Socket soc = cli.Client;
                NetworkStream ns = cli.GetStream();

                if (!MessageBuilder.ReceiveMessage(ns, out byte[] msg, timeout)) throw new Exception("Failed to get intent request message.");
                MessageBuilder.DeserializeMessage(msg, out string msgType, out string tgt, out object value);
                if (!MessageBuilder.ValidateRequestMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);
                if ((string)value != "intent") throw new Exception(unexpectedMsgErr);

                MessageBuilder.SendMessage(MessageBuilder.Message("ret", string.Empty, $"get;{clientId}"), ns);

                if (!MessageBuilder.ReceiveMessage(ns, out msg, timeout)) throw new Exception("Failed to get server response.");
                MessageBuilder.DeserializeMessage(msg, out msgType, out tgt, out value);
                if (MessageBuilder.ValidateErrorMessage(msgType, tgt, value))
                {
                    string[] parts = ((string)value).Split(";");
                    if (parts.Length != 2) throw new Exception("Recieved an invalid error message.");
                    clientId = uint.Parse(parts[0]);
                    UpdateClientIDFile();
                    Log.LogEvent($"Batch server denied new batch. Reason: {parts[1]}", "GetBatch");
                    cli.Close();
                    if (parts[1] == "NoAvailableBatches")
                        return GetBatchStatus.NoAvailableBatches;
                    else if (parts[1] == "LimitReached")
                        return GetBatchStatus.LimitReached;
                    else
                        return GetBatchStatus.UnspecifiedError;
                }
                else if (!MessageBuilder.ValidateDataMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);

                //generated batches are relatively small (~10k) so it's not too bad to use the normal messaging system
                byte[] bytes = (byte[])value;
                File.WriteAllBytes(Path.Combine(Globals.cacheDir, "batchDownload.7z.tmp"), bytes); //extraction is done externally

                MessageBuilder.SendMessage(MessageBuilder.Message("ack", string.Empty, string.Empty), ns);
                ns.Close();
                cli.Close();
            }
            catch (Exception e)
            {
                Log.LogException("Failed to get new batch.", "GetBatch", e);
                try { cli?.Close(); } catch { }
                return GetBatchStatus.UnspecifiedError;
            }

            return GetBatchStatus.Success;
        }
        public static ReturnBatchStatus ReturnBatch(string path, TimeSpan timeout)
        {
            if (!initialized) throw new Exception("Attempted to use uninitialized BatchManager.");

            Log.LogEvent($"Attempting to return batch to '{serverIP}:{serverPort}'.", "ReturnBatch");
            if (!uint.TryParse(Path.GetFileName(path), out uint batchNum))
            {
                Log.LogEvent(Log.EventType.Error, $"Failed to determine batch number for path '{path}'", "ReturnBatch");
                return ReturnBatchStatus.CouldNotDetermineBatchNum;
            }
            TcpClient cli = new();

            try
            {
                string filePath = Path.Combine(Globals.cacheDir, "batchUpload.7z.tmp");
                if (!SevenZip.TryCompress7z(path, filePath)) throw new Exception("Failed to compress batch.");

                cli.Connect(serverIP, serverPort);
                if (!cli.Connected) throw new Exception(failedConnErr);
                Socket soc = cli.Client;
                NetworkStream ns = cli.GetStream();

                if (!MessageBuilder.ReceiveMessage(ns, out byte[] msg, timeout)) throw new Exception("Failed to get intent request message.");
                MessageBuilder.DeserializeMessage(msg, out string msgType, out string tgt, out object value);
                if (!MessageBuilder.ValidateRequestMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);
                if ((string)value != "intent") throw new Exception(unexpectedMsgErr);

                MessageBuilder.SendMessage(MessageBuilder.Message("ret", string.Empty, $"ret;{clientId}"), ns);

                if (!MessageBuilder.ReceiveMessage(ns, out msg, timeout)) throw new Exception("Failed to get batchNum request message.");
                MessageBuilder.DeserializeMessage(msg, out msgType, out tgt, out value);
                if (MessageBuilder.ValidateErrorMessage(msgType, tgt, value))
                {
                    string[] parts = ((string)value).Split(";");
                    if (parts.Length != 2) throw new Exception("Recieved an invalid error message.");
                    clientId = 0;
                    UpdateClientIDFile();
                    Log.LogEvent($"Batch server denied returning of batch. Reason: {parts[1]}", "ReturnBatch");
                    cli.Close();
                    if (parts[1] == "InvalidId")
                        return ReturnBatchStatus.InvalidId;
                    else
                        return ReturnBatchStatus.UnspecifiedError;
                }
                else if (!MessageBuilder.ValidateRequestMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);

                if ((string)value != "batchNum") throw new Exception(unexpectedMsgErr);

                MessageBuilder.SendMessage(MessageBuilder.Message("ret", string.Empty, $"{batchNum}"), ns);

                if (!MessageBuilder.ReceiveMessage(ns, out msg, timeout)) throw new Exception("Failed to get batch confirmation message.");
                MessageBuilder.DeserializeMessage(msg, out msgType, out tgt, out value);
                if (MessageBuilder.ValidateErrorMessage(msgType, tgt, value))
                {
                    string[] parts = ((string)value).Split(";");
                    if (parts.Length != 1) throw new Exception("Recieved an invalid error message.");
                    Log.LogEvent($"Batch server denied returning of batch. Reason: {parts[0]}", "ReturnBatch");
                    cli.Close();
                    if (parts[0] == "BatchNotAssigned")
                        return ReturnBatchStatus.BatchNotAssigned;
                    else
                        return ReturnBatchStatus.UnspecifiedError;
                }
                else if (!MessageBuilder.ValidateRequestMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);

                if ((string)value != $"{batchNum}") throw new Exception($"Received batchNum does not match actual batchNum: {value}=/={batchNum}.");

                //completed batches are huge (200MB+) so a different tranfer method is used
                FileStream fs = File.OpenRead(filePath);
                MessageBuilder.SendStreamData(fs, ns, timeout.Milliseconds);
                fs.Close();

                if (!MessageBuilder.ReceiveMessage(ns, out msg)) throw new Exception("Failed to get acknowledgement message.");
                MessageBuilder.DeserializeMessage(msg, out msgType, out tgt, out value);
                if (!MessageBuilder.ValidateAckMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);

                //Sucessfully transmitted, free to delete local copy
                ns.Close();
                cli.Close();
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                Log.LogException("Failed to return batch.", "ReturnBatch", e);
                try { cli?.Close(); } catch { }
                return ReturnBatchStatus.UnspecifiedError;
            }

            return ReturnBatchStatus.Success;
        }
        public static RegetBatchesStatus RegetAllBatches(TimeSpan timeout)
        {
            if (!initialized) throw new Exception("Attempted to use uninitialized BatchManager.");

            Log.LogEvent($"Attempting to reget all batches from '{serverIP}:{serverPort}'.", "RegetAllBatches");

            TcpClient cli = new();

            try
            {
                cli.Connect(serverIP, serverPort);
                if (!cli.Connected) throw new Exception(failedConnErr);
                Socket soc = cli.Client;
                NetworkStream ns = cli.GetStream();

                if (!MessageBuilder.ReceiveMessage(ns, out byte[] msg, timeout)) throw new Exception("Failed to get intent request message.");
                MessageBuilder.DeserializeMessage(msg, out string msgType, out string tgt, out object value);
                if (!MessageBuilder.ValidateRequestMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);
                if ((string)value != "intent") throw new Exception(unexpectedMsgErr);

                MessageBuilder.SendMessage(MessageBuilder.Message("ret", string.Empty, $"reget;{clientId}"), ns);

                if (!MessageBuilder.ReceiveMessage(ns, out msg, timeout)) throw new Exception("Failed to get server response.");
                MessageBuilder.DeserializeMessage(msg, out msgType, out tgt, out value);
                if (MessageBuilder.ValidateErrorMessage(msgType, tgt, value))
                {
                    string[] parts = ((string)value).Split(";");
                    if (parts.Length != 2) throw new Exception("Recieved an invalid error message.");
                    clientId = uint.Parse(parts[0]);
                    UpdateClientIDFile();
                    Log.LogEvent($"Batch server denied new batch. Reason: {parts[1]}", "GetBatch");
                    cli.Close();
                    if (parts[1] == "InvalidId")
                        return RegetBatchesStatus.InvalidId;
                    else if (parts[1] == "NoBatchesAssigned")
                        return RegetBatchesStatus.NoBatchesAssigned;
                    else
                        return RegetBatchesStatus.UnspecifiedError;
                }
                else if (!MessageBuilder.ValidateDataMessage(msgType, tgt, value)) throw new Exception(unexpectedMsgErr);

                //generated batches are relatively small (~10k) so it's not too bad to use the normal messaging system
                //since they should come in small amounts (doubt more than 10), data should be limited to ~100k, which is a already a bit but should not be too bad still
                byte[] bytes = (byte[])value;
                File.WriteAllBytes(Path.Combine(Globals.cacheDir, "batchDownload.7z.tmp"), bytes); //extraction is done externally

                MessageBuilder.SendMessage(MessageBuilder.Message("ack", string.Empty, string.Empty), ns);
                ns.Close();
                cli.Close();
            }
            catch (Exception e)
            {
                Log.LogException("Failed to get assigned batches.", "RegetAllBatches", e);
                try { cli?.Close(); } catch { }
                return RegetBatchesStatus.UnspecifiedError;
            }

            return RegetBatchesStatus.Success;
        }


        private static void UpdateClientIDFile()
        {
            string idPath = Path.Combine(Globals.homeDir, "clientId.cfg");

            if (File.Exists(idPath))
            {
                string[] lines = File.ReadAllLines(idPath);

                for (int i = 0; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;
                    lines[i] = lines[i].Trim();
                    if (lines[i].Trim().StartsWith("//")) continue;

                    if (lines[i].StartsWith("clientId="))
                        lines[i] = $"clientId={clientId}";
                    else continue;
                }

                File.WriteAllLines(idPath, lines);
            }
            else
            {
                File.WriteAllText(idPath, $"clientId={clientId}");
            }
        }



        public enum GetBatchStatus
        {
            UnspecifiedError = -1,
            Success = 0,
            LimitReached,
            NoAvailableBatches,
        }
        public enum ReturnBatchStatus
        {
            UnspecifiedError = -1,
            Success = 0,
            InvalidId,
            BatchNotAssigned,
            CouldNotDetermineBatchNum,
        }
        public enum RegetBatchesStatus
        {
            UnspecifiedError = -1,
            Success = 0,
            NoBatchesAssigned, //no batches assigned sort of counts as success since technically all assigned batches were resent
                               //and the getting of more batches will be done further down the work loop, however, to get extraction
                               //done properly outside BatchManager, a separate signal is sent for simplicity
            InvalidId,
        }
    }
}
