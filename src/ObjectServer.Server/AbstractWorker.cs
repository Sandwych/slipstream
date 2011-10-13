using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZMQ;

namespace ObjectServer.Server
{
    public abstract class AbstractWorker
    {
        private readonly Socket broadcastSocket = new Socket(SocketType.SUB);

        public AbstractWorker()
        {
            this.ID = Guid.NewGuid();

            this.broadcastSocket.Connect(Environment.Configuration.BroadcastUrl);
            this.broadcastSocket.Subscribe("STOP", Encoding.UTF8);
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
    }
}
