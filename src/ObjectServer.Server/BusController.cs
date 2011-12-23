using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ZMQ;

namespace ObjectServer.Server
{
    public sealed class BusController : IDisposable
    {
        private readonly Socket broadcastSocket = new Socket(SocketType.PUB);
        private volatile bool started = false;
        private bool disposed = false;


        ~BusController()
        {
            this.Dispose(false);
        }

        public void Start()
        {
            var address = SlipstreamEnvironment.Settings.BroadcastUrl;

            this.broadcastSocket.Bind(address);
            this.started = true;
        }

        public void BeginStopAll()
        {
            if (this.started)
            {
                this.BeginStopRpcWorkers();
                this.BeginStopHttpServer();
                this.started = false;
            }
        }

        public void BeginStopRpcWorkers()
        {
            if (this.started)
            {
                this.Broadcast("STOP-RPC");
            }
        }

        public void BeginStopHttpServer()
        {
            if (this.started)
            {
                this.Broadcast("STOP-HTTPD");
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
                () => String.Format("Sending broadcast command: [{0}]", command));

            this.broadcastSocket.Send(command, Encoding.UTF8);
        }

        private void Dispose(bool isDisposing)
        {
            if (!this.disposed)
            {
                if (isDisposing)
                {
                    //释放托管资源
                }

                //释放非托管资源
                this.BeginStopAll();
                this.broadcastSocket.Dispose();

                this.disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
