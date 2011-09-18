using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Threading;

using Kayak;
using Kayak.Http;

using ObjectServer;

namespace ObjectServer.Http
{

    /// <summary>
    /// HTTP RPC 接口服务器
    /// </summary>
    public sealed class HttpServer : IDisposable
    {
        private readonly ZMQ.Socket commandSocket = new ZMQ.Socket(ZMQ.SocketType.SUB);
        private readonly ZMQ.Socket rpcSocket = new ZMQ.Socket(ZMQ.SocketType.REQ);
        private readonly string rpcReqUrl;
        private readonly IPEndPoint httpServerEndPoint;
        private readonly IScheduler scheduler;
        private bool disposed = false;
        private readonly Thread httpdThread;

        public HttpServer(string controllerUrl, string rpcHostUrl, int listenPort)
        {
            LoggerProvider.EnvironmentLogger.Info("Starting HTTP Server...");

            if (string.IsNullOrEmpty(controllerUrl))
            {
                throw new ArgumentNullException("controllerUrl");
            }

            if (string.IsNullOrEmpty(rpcHostUrl))
            {
                throw new ArgumentNullException("rpcHostUrl");
            }

            if (listenPort <= 0 || listenPort >= UInt16.MaxValue)
            {
                throw new ArgumentOutOfRangeException("listenPort");
            }

            commandSocket.Connect(controllerUrl);
            commandSocket.Subscribe("STOP", Encoding.UTF8);

            this.httpServerEndPoint = new IPEndPoint(IPAddress.Any, listenPort);

            this.rpcReqUrl = rpcHostUrl;
            LoggerProvider.EnvironmentLogger.Info("Connecting to MQ: [" + this.rpcReqUrl + "]");

            this.scheduler = KayakScheduler.Factory.Create(new SchedulerDelegate(this));
            this.httpServerEndPoint = new IPEndPoint(IPAddress.Any, listenPort);

            this.httpdThread = new Thread(new ThreadStart(this.DoHttpServer));
        }

        ~HttpServer()
        {
            this.Dispose(false);
        }

        public void Start()
        {
            Debug.Assert(this.rpcSocket != null);

            this.rpcSocket.Connect(this.rpcReqUrl);

            this.httpdThread.Start();
            this.WaitForStopCommand();

            LoggerProvider.EnvironmentLogger.Debug(() => "Waiting The HTTP Server thread to stop...");
            var joinResult = this.httpdThread.Join(20000);
            if (joinResult)
            {
                LoggerProvider.EnvironmentLogger.Debug(() => "The HTTP Server thread is joined.");
            }
            else
            {
                LoggerProvider.EnvironmentLogger.Warn(
                    "The HTTP Server cannot stop itself, we will kill it...");
                this.httpdThread.Abort();
            }
        }

        private void WaitForStopCommand()
        {

            while (true)
            {
                var cmd = this.commandSocket.Recv(Encoding.UTF8);

                if (cmd == "STOP")
                {
                    LoggerProvider.EnvironmentLogger.Info(
                        "'STOP' command received, try to stop the HTTP Server...");
                    this.scheduler.Stop();
                    break;
                }
            }

        }

        private void DoHttpServer()
        {
            LoggerProvider.EnvironmentLogger.Info("Initializing Kayak HTTP Server...");

            var reqDel = new RequestDelegate(this.rpcSocket);

            using (var server = KayakServer.Factory.CreateHttp(reqDel, this.scheduler))
            using (server.Listen(this.httpServerEndPoint))
            {
                // runs scheduler on calling thread. this method will block until
                // someone calls Stop() on the scheduler.
                LoggerProvider.EnvironmentLogger.Info("Starting Kayak HTTP Server...");
                this.scheduler.Start();
            }

            LoggerProvider.EnvironmentLogger.Info("The HTTP server is stopped.");
        }

        #region IDisposable Members

        private void Dispose(bool isDisposing)
        {
            if (!this.disposed)
            {

                LoggerProvider.EnvironmentLogger.Info("Disposing HTTP Server...");
                if (isDisposing)
                {
                    //释放托管资源
                }

                //释放非托管资源

                this.scheduler.Stop();

                this.scheduler.Dispose();
                this.rpcSocket.Dispose();
                this.commandSocket.Dispose();

                this.disposed = true;
                LoggerProvider.EnvironmentLogger.Info("The HTTP Server has been closed.");
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
