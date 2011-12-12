using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ObjectServer;

namespace ObjectServer.Server
{
    public sealed class ServerProcess : IDisposable
    {
        private bool disposed = false;
        private Server.Supervisor m_supervisor = null;

        ~ServerProcess()
        {
            this.Dispose(false);
        }

        public void Run()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("ServerProcess");
            }

            var cfg = SlipstreamEnvironment.Configuration;

            if (cfg.Role == ServerRoles.Standalone || cfg.Role == ServerRoles.Supervisor)
            {
                this.m_supervisor = new Supervisor();
                this.m_supervisor.Start();
            }

            Thread.Sleep(1000); //去掉此处，改为回报模式
            if (cfg.Role == ServerRoles.Standalone || cfg.Role == ServerRoles.Worker)
            {
                var rpcHostWorker = StartApplicationServer();
            }

            Thread.Sleep(1000); //去掉此处，改为回报模式
            if (cfg.Role == ServerRoles.Standalone || cfg.Role == ServerRoles.HttpServer)
            {
                var httpThread = StartHttpServer();
            }
        }

        public void BeginStopAll()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("ServerProcess");
            }

            if (this.m_supervisor == null)
            {
                throw new InvalidOperationException();
            }

            this.m_supervisor.BeginStopAll();
        }

        public void BeginStopRpcWorkers()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("ServerProcess");
            }

            if (this.m_supervisor == null)
            {
                throw new InvalidOperationException();
            }

            this.m_supervisor.BeginStopRpcWorkers();
        }

        public void BeginStopHttpServer()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("ServerProcess");
            }

            if (this.m_supervisor == null)
            {
                throw new InvalidOperationException();
            }

            this.m_supervisor.BeginStopHttpServer();
        }

        private static RpcBusWorker StartApplicationServer()
        {
            LoggerProvider.EnvironmentLogger.Info("Starting application server...");

            var rpcHostWorker = new RpcBusWorker();
            rpcHostWorker.Start();

            Console.WriteLine("Application server is started.");
            return rpcHostWorker;
        }

        private static Thread StartHttpServer()
        {
            LoggerProvider.EnvironmentLogger.Info("Starting HTTP server...");

            var serverThread = new Thread(() =>
            {
                using (var cs = new ObjectServer.Http.AnnaHttpServer(
                    SlipstreamEnvironment.Configuration.BroadcastUrl,
                    SlipstreamEnvironment.Configuration.RpcBusUrl,
                    SlipstreamEnvironment.Configuration.HttpListenUrl))
                {
                    cs.Start();
                }
            });
            serverThread.Start();
            LoggerProvider.EnvironmentLogger.Info("HTTP server is started.");
            return serverThread;
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                //释放非托管资源
                if (disposing)
                {
                }

                //释放托管资源
                var role = SlipstreamEnvironment.Configuration.Role;
                if (role == ServerRoles.Standalone || role == ServerRoles.Supervisor)
                {
                    this.m_supervisor.Dispose();
                }

                this.disposed = true;
            }
        }

        #endregion
    }
}
