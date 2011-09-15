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
        private readonly ZMQ.Socket controllerSocket = new ZMQ.Socket(ZMQ.SocketType.SUB);
        private readonly string rpcReqUrl;
        private readonly IPEndPoint httpEndPoint;
        private readonly ZMQ.Socket zsocket = new ZMQ.Socket(ZMQ.SocketType.REQ);
        private readonly KayakScheduler scheduler = new KayakScheduler(new SchedulerDelegate());

        public HttpServer(string controllerUrl, string rpcHostUrl, int listenPort)
        {
            LoggerProvider.RpcLogger.Info("Starting HTTP Server...");

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

            controllerSocket.Connect(controllerUrl);
            controllerSocket.Subscribe("STOP", Encoding.UTF8);

            this.httpEndPoint = new IPEndPoint(IPAddress.Any, listenPort);

            this.rpcReqUrl = rpcHostUrl;
            LoggerProvider.RpcLogger.Info("Connecting to MQ: [" + this.rpcReqUrl + "]");
        }

        ~HttpServer()
        {
            if (this.zsocket != null)
            {
                this.zsocket.Dispose();
            }
        }

        public void Start()
        {
            Debug.Assert(this.zsocket != null);

            this.zsocket.Connect(this.rpcReqUrl);

            var threadProc = new ThreadStart(() =>
            {
                this.DoHttpServer();
            });

            var thread = new Thread(threadProc);
            thread.Start();
            this.WaitForStopCommand();
            thread.Join();
        }

        private void WaitForStopCommand()
        {

            while (true)
            {
                var cmd = this.controllerSocket.Recv(Encoding.UTF8);

                if (cmd == "STOP")
                {
                    LoggerProvider.RpcLogger.Info("'STOP' command received, try to stop the HTTP Server...");
                    scheduler.Stop();
                    break;
                }
            }

            LoggerProvider.RpcLogger.Info("HTTP Server is stopped.");
        }

        private void DoHttpServer()
        {
            LoggerProvider.RpcLogger.Info("Initializing Kayak HTTP Server...");
            scheduler.Post(() =>
            {
                KayakServer
                    .Factory
                    .CreateHttp(new RequestDelegate(this.zsocket))
                    .Listen(new IPEndPoint(IPAddress.Any, 9287));
            });

            // runs scheduler on calling thread. this method will block until
            // someone calls Stop() on the scheduler.
            LoggerProvider.RpcLogger.Info("Starting Kayak HTTP Server...");
            scheduler.Start();
        }

        #region IDisposable Members

        public void Dispose()
        {
            LoggerProvider.RpcLogger.Info("Disposing MQ...");
            this.zsocket.Dispose();
        }

        #endregion
    }
}
