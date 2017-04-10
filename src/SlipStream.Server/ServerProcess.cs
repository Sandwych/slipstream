using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

using SlipStream;

namespace SlipStream.Server
{
    public sealed class ServerProcess : IDisposable
    {
        private bool disposed = false;
        private Server.BusController _busController = null;

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

            var cfg = SlipstreamEnvironment.Settings;

            if (cfg.Role == ServerRoles.Standalone || cfg.Role == ServerRoles.Controller)
            {
                this._busController = new BusController();
                this._busController.Start();
            }

            //REVIEW
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

            if (this._busController == null)
            {
                throw new InvalidOperationException();
            }

            this._busController.BeginStopAll();
        }

        public void BeginStopRpcWorkers()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("ServerProcess");
            }

            if (this._busController == null)
            {
                throw new InvalidOperationException();
            }

            this._busController.BeginStopRpcWorkers();
        }

        public void BeginStopHttpServer()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("ServerProcess");
            }

            if (this._busController == null)
            {
                throw new InvalidOperationException();
            }

            this._busController.BeginStopHttpServer();
        }

        private ServiceBusWorker StartApplicationServer()
        {
            LoggerProvider.EnvironmentLogger.Info("Starting application server...");

            var rpcHostWorker = new ServiceBusWorker();

            rpcHostWorker.Start();

            Console.WriteLine("Application server is started.");
            return rpcHostWorker;
        }

        private Thread StartHttpServer()
        {
            LoggerProvider.EnvironmentLogger.Info("Starting HTTP server...");

            var serverThread = new Thread(() =>
            {
                using (var cs = new SlipStream.Http.HttpServer(
                    SlipstreamEnvironment.Settings.BroadcastUrl,
                    SlipstreamEnvironment.Settings.RpcBusUrl,
                    SlipstreamEnvironment.Settings.HttpListenUrl))
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
                var role = SlipstreamEnvironment.Settings.Role;
                if (role == ServerRoles.Standalone || role == ServerRoles.Controller)
                {
                    this._busController.Dispose();
                }

                this.disposed = true;
            }
        }

        #endregion
    }
}
