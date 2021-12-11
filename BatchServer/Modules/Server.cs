using System;
using System.Collections.Generic;
using System.IO;

using MySql.Data.MySqlClient;
using MySql.Data.Types;

using DidasUtils;
using DidasUtils.Net;
using DidasUtils.Logging;
using DidasUtils.Files;
using DidasUtils.ErrorCorrection;

using BatchServer;
using BatchServer.Messages;

namespace BatchServer.Modules
{
    public class Server
    {
        private readonly Dictionary<Client, ServeStatus> handling;



        public Server()
        {
            handling = new Dictionary<Client, ServeStatus>();
        }



        public void Handle(Client client)
        {
            client.messageReceived += Message_Received;

            lock (handling)
            {
                handling.Add(client, new ServeStatus());
            }

            client.SendMessage(new Message_Server_StateRequest().Serialize());
        }



        private void HandleRequest(Client client, Message_Client_StateRequest msg)
        {
            CheckUserId(msg.userId, client, out int user_id);

            switch (msg.request)
            {
                case Message_Client_StateRequest.Request.RequestBatches:

                    bool allocSuccess = AllocateBatches(user_id, msg.amount, out string[] batches, out Message_Server_Serve.Status status);

                    lock (handling)
                    {
                        handling[client].allocatedBatches = batches;
                        handling[client].status = 2;
                    }

                    client.SendMessage(new Message_Server_Serve(user_id, status).Serialize());

                    if (!allocSuccess) SafeDisconnect(client);

                    break;

                    //FIXME: Add missing

                default:
                    Log.LogEvent(Log.EventType.Warning, $"Client '{client.socket.Client.RemoteEndPoint}' stated invalid request '{(byte)msg.request}'.", "Server");
                    SafeDisconnect(client);
                    return;
            }

            handling[client].request = msg.request;

            throw new NotImplementedException(); //FIXME 
            return;
        }
        private void ClientAcknowledgedServe(Client client, Message_Client_Acknowledge msg)
        {
            Message_Client_StateRequest.Request request;

            lock (handling)
            {
                request = handling[client].request;
            }

            switch (request)
            {
                case Message_Client_StateRequest.Request.RequestBatches:

                    string[] batches;
                    lock (handling)
                    {
                        batches = handling[client].allocatedBatches;
                    }

                    List<byte> bytes = new();

                    foreach (string b in batches)
                        bytes.AddRange(File.ReadAllBytes(b));

                    lock (handling)
                    {
                        handling[client].status = 4;
                    }
                    client.SendMessage(new Message_Server_Data(ErrorProtectedBlock.ProtectDataToArray(bytes.ToArray(), ErrorProtectedBlock.ErrorProtectionType.Fletcher32, 16384)).Serialize());

                    break;

                //FIXME: Add missing

                default:
                    Log.LogEvent(Log.EventType.Warning, $"Client '{client.socket.Client.RemoteEndPoint}' had an invalid request '{(byte)request}'.", "Server");
                    SafeDisconnect(client);
                    return;
            }

            throw new NotImplementedException(); //FIXME 
            return;
        }
        private bool CheckUserId(int userId, Client client, out int user_id)
        {
            if (userId <= 0) //check if no userId stored in client
            {
                user_id = AddNewUser(client.socket.Client.RemoteEndPoint.ToString());
                return false;
            }
            else
            {
                //check if user_id exists
                MySqlDataReader ret = Globals.Db.SendCommandDataReader($"SELECT EXISTS(SELECT * FROM users WHERE user_id={userId});");
                if (ret.GetInt32($"EXISTS(SELECT * FROM users WHERE user_id={userId})") == 0)
                {
                    user_id = AddNewUser(client.socket.Client.RemoteEndPoint.ToString());
                    ret.Dispose();
                    return false;
                }
                else
                {
                    user_id = userId;
                    ret.Dispose();
                    return true;
                }
            }
        }
        private int AddNewUser(string ip)
        {
            MySqlDateTime now = new (DateTime.Now);

            Globals.Db.SendCommandNonQuery($"INSERT INTO users(last_ip,last_contacted) VALUES({ip},{now});");
            int ret = (int)Globals.Db.SendCommandScalar("SELECT user_id FROM users ORDER BY user_id DESC LIMIT 1;");

            Log.LogEvent($"Created user_id {ret} for client '{ip}'.", "Server");

            return ret;
        }
        private bool AllocateBatches(int userId, int amount, out string[] batches, out Message_Server_Serve.Status status)
        {
            batches = null;

            List<string> paths = new();

            int count = Math.Min(amount, Globals.maxPerClient);//FIXME: doesn't check for already allocated batches to user

            MySqlDataReader ret = Globals.Db.SendCommandDataReader($"SELECT job_start FROM jobs WHERE status=1 ORDER BY job_start DESC LIMIT {count};");

            if (!ret.HasRows)
            {
                status = Message_Server_Serve.Status.Err_NoAvailableBatches;
                ret.Dispose();
                return false;
            }

            while (ret.Read())
            {
                paths.Add(ret.GetString("job_start"));
            }

            ret.Dispose();
            batches = paths.ToArray();
            status = Message_Server_Serve.Status.SendingBatches;

            string indices = string.Empty;
            foreach (string s in batches)
                indices += $" OR job_start={Path.GetFileNameWithoutExtension(s)}";

            Globals.Db.SendCommandNonQuery($"UPDATE jobs SET status=2, assigned_user={userId} WHERE {indices[4..]};");//index 4 = end of " OR " at the beggining

            return true;
        }



        private void Message_Received(Client sender, byte[] data)
        {
            byte status = 255; 

            try
            {
                lock (handling)
                {
                    status = handling[sender].status;
                }

                Message msg = Message.Deserialize(data);

                switch (status)
                {
                    case 0:
                        if (msg is not Message_Client_StateRequest req) SafeDisconnect(sender);
                        else HandleRequest(sender, req);
                        break;

                    case 2:
                        if (msg is not Message_Client_Acknowledge ack) SafeDisconnect(sender);
                        else ClientAcknowledgedServe(sender, ack);
                        break;

                        //FIXME: Add missing ones

                    default:
                        Log.LogEvent(Log.EventType.Warning, $"Client '{sender.socket.Client.RemoteEndPoint}' has invalid status {status}. Disconnecting", "Server");
                        
                        break;
                }
            }
            catch (Exception e)
            {
                Log.LogException($"Error serving client with status ${status}", "Server", e);

                SafeDisconnect(sender);
            }
        }
        private void SafeDisconnect(Client client)
        {
            try
            {
                try { client.Disconnect(); } catch { }
                lock (handling) handling.Remove(client);
            }
            catch { }
        }



        private class ServeStatus
        {
            public byte status;
            public string[] allocatedBatches;
            public Message_Client_StateRequest.Request request;



            public ServeStatus()
            {
                status = 0;
                allocatedBatches = null;
            }
        }
    }
}
