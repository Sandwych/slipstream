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

        public void StopAll()
        {
            if (this.started)
            {
                this.Broadcast("STOP");
                this.started = false;
            }
        }

        public void Broadcast(string command)
        {
            Debug.Assert(this.started);

            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentNullException("command");
            }

            LoggerProvider.EnvironmentLogger.Debug(
                () => String.Format("Broadcasting command: [{0}]", command));

            this.socket.Send(command, Encoding.UTF8);
        }

        public void Dispose()
        {
            this.StopAll();
        }
    }
}
