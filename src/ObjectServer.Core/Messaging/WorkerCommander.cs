using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ZMQ;

namespace ObjectServer.Messaging
{
    public sealed class WorkerCommander : IDisposable
    {
        private readonly Socket socket = new Socket(SocketType.PUB);
        private bool started = false;

        public void Start()
        {
            var address = Environment.Configuration.CommanderUrl;

            this.socket.Bind(address);
            this.started = true;
        }

        public void Stop()
        {
            if (started)
            {
                this.Broadcast("STOP");
                this.socket.Dispose();
            }
        }

        public void Broadcast(string command)
        {
            Debug.Assert(this.started);

            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentNullException("command");
            }

            this.socket.Send(command, Encoding.UTF8);
        }

        public void Dispose()
        {

            this.Stop();
        }
    }
}
