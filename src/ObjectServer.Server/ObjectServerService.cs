using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

using ObjectServer;

namespace ObjectServer.Server
{
    public partial class ObjectServerService : ServiceBase
    {
        private Thread serverThread = null;          

        public ObjectServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Debug.Assert(this.serverThread == null);

            var cs = new RpcHost();
            this.serverThread = new Thread(cs.Start);
            this.serverThread.Start();
        }

        protected override void OnStop()
        {
            Debug.Assert(this.serverThread != null);

            this.serverThread.Abort();
        }
    }
}
