using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using DidasUtils.Logging;

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



        public static void ListenAndMerge()
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
            NetworkStream ns = client.GetStream();
            byte[] buffer = new byte[1024];
            string message = string.Empty;
            int attempts;
            bool handleDone = false;

            while (!handleDone)
            {
                attempts = 0; 

                while (attempts < 1000)
                {
                    if (!client.Connected)
                    {
                        ns.Dispose();
                        client.Close();
                    }

                    if (ns.Length > ns.Position) //has something to read
                    {
                        int trueRead = ns.Read(buffer, 0, buffer.Length);
                        message += Encoding.Unicode.GetString(buffer, 0, trueRead);

                        if (TryProcessMessage(message))
                            handleDone = true;
                    }

                    attempts++;
                    Thread.Sleep(1);
                }
            }
        }
        private static bool TryProcessMessage(string mesage)
        {
            if (mesage.Length < 3)
                throw new Exception($"Message '{mesage}' is too short.");
        }
    }
}
