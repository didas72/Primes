using System;
using System.Collections.Generic;
using System.Threading;

using DidasUtils;
using DidasUtils.Data;
using DidasUtils.ErrorCorrection;
using DidasUtils.Net;
using DidasUtils.Logging;

using Primes.Common.Net.Messages;

namespace BatchServer.Modules
{
    public class ControlHandler
    {
        public bool IsRunning { get => (thread != null && thread.IsAlive); }



        private Thread thread;
        private readonly Queue<Message> pendingHandles;



        public ControlHandler()
        {
            pendingHandles = new Queue<Message>();
        }



        public void Handle(Client client)
        {
            if (IsRunning) return;

            pendingHandles.Clear();

            thread = new Thread(() => HandleControl(client));
            thread.Start();
        }



        private void HandleControl(Client client)
        {
            try
            {
                client.messageReceived += MessageReceived;

                int tries;
                client.SendMessage(new Message_Server_StateControl().Serialize());

                tries = 50;

                while (tries > 0)
                {
                    lock (pendingHandles)
                        if (pendingHandles.Count != 0) break;

                    tries--;
                }

                if (tries == 0)
                {
                    client.Disconnect();
                    return;
                }

                Message msg = pendingHandles.Dequeue();

                if (msg is not Message_Control_Control ctl) { client.Disconnect(); return; }

                switch (ctl.control)
                {
                    case Message_Control_Control.Control.StopServer:
                        client.Disconnect();
                        throw new NotImplementedException(); //FIXME
                        break;

                    default:
                        client.Disconnect();
                        break;
                }
            }
            catch (Exception e)
            {
                Log.LogException("Error handling control.", "ControlHandler", e);
                return;
            }

            Log.LogEvent($"Finished handling controller.", "ControlHandler");
        }

        private void MessageReceived(Client sender, byte[] data) { lock (pendingHandles) pendingHandles.Enqueue(Message.Deserialize(data)); }
    }
}
