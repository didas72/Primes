using System;
using System.Collections.Generic;
using System.Threading;

using DidasUtils;
using DidasUtils.Net;

namespace BatchServer.Modules
{
    public class Server
    {
        public bool IsRunning { get => (thread != null && thread.IsAlive); }



        private Thread thread;



        public Server()
        {

        }



        public void Handle(Client client) { throw new NotImplementedException(); }
    }
}
