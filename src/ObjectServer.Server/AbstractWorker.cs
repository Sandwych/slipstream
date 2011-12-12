using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZMQ;

namespace ObjectServer.Server
{
    public abstract class AbstractWorker : IDisposable
    {
        private readonly Socket broadcastSocket = new Socket(SocketType.SUB);
        private bool m_disposed = false;

        public AbstractWorker(string stopCommand)
        {
            this.ID = Guid.NewGuid();

            this.broadcastSocket.Connect(SlipstreamEnvironment.Configuration.BroadcastUrl);
            this.broadcastSocket.Subscribe(stopCommand, Encoding.UTF8);
        }

        ~AbstractWorker()
        {
            this.Dispose(false);
        }

        public void Start()
        {
            var msg = String.Format("Starting worker: ID=[{0}]", this.ID);
            LoggerProvider.EnvironmentLogger.Info(msg);
            this.OnStart();
        }

        public void Abort()
        {
            var msg = String.Format("Aborting worker: ID=[{0}]", this.ID);
            LoggerProvider.EnvironmentLogger.Warn(msg);
            this.OnAbort();
        }

        public Guid ID { get; private set; }

        protected abstract void OnStart();
        protected abstract void OnAbort();

        protected string ReceiveControlCommand()
        {
            return this.broadcastSocket.Recv(Encoding.UTF8);
        }

        #region IDisposable Members

        public void Dispose(bool isDisposing)
        {
            if (!this.m_disposed)
            {
                if (isDisposing) //释放非托管资源
                {
                    this.broadcastSocket.Dispose();
                }
                //释放非托管资源

                this.m_disposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion
    }
}
