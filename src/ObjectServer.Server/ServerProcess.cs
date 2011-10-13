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
        private readonly Server.Supervisor m_supervisor = new Server.Supervisor();

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

            this.m_supervisor.Start();

            var cfg = Environment.Configuration;
            if (cfg.Role == ServerRoles.Standalone || cfg.Role == ServerRoles.Worker)
            {
                var rpcHostWorker = StartApplicationServer();
            }

            if (cfg.Role == ServerRoles.Standalone || cfg.Role == ServerRoles.HttpServer)
            {
                var httpThread = StartHttpServer();
            }
        }

        public void BeginStop()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("ServerProcess");
            }

            this.m_supervisor.StopAll();
        }

        private static RpcHostWorker StartApplicationServer()
        {
            LoggerProvider.EnvironmentLogger.Info("Starting application server...");

            var rpcHostWorker = new RpcHostWorker();
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
                    Environment.Configuration.BroadcastUrl,
                    Environment.Configuration.RpcHostUrl,
                    Environment.Configuration.HttpListenUrl))
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
                this.m_supervisor.Dispose();

                this.disposed = true;
            }
        }

        #endregion
    }
}
