using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ZMQ;

namespace ObjectServer.Server
{
    public sealed class WorkerCommander : IDisposable
    {
        private readonly Socket socket = new Socket(SocketType.PUB);
        private bool started = false;
        private bool disposed = false;


        ~WorkerCommander()
        {
            this.Dispose(false);
        }

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

        private void Dispose(bool isDisposing)
        {
            if (!this.disposed)
            {
                if (isDisposing)
                {
                    //释放托管资源
                }

                //释放非托管资源
                this.StopAll();
                this.socket.Dispose();

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
