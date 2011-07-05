using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Kayak;
using Kayak.Http;

namespace ObjectServer.Http
{

    /// <summary>
    /// HTTP RPC 接口服务器
    /// </summary>
    public sealed class HttpServer : IDisposable
    {

        private readonly string rpcHostUrl;
        private readonly IPEndPoint httpEndPoint;
        private ZMQ.Socket zsocket;

        public HttpServer(int listenPort)
        {
            
            this.httpEndPoint = new IPEndPoint(IPAddress.Any, listenPort);

            this.rpcHostUrl = "tcp://127.0.0.1:5555";
            this.zsocket = new ZMQ.Socket(ZMQ.SocketType.REQ);
            this.zsocket.Connect(this.rpcHostUrl);
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
            var scheduler = new KayakScheduler(new SchedulerDelegate());
            scheduler.Post(() =>
            {
                KayakServer
                    .Factory
                    .CreateHttp(new RequestDelegate(this.zsocket))
                    .Listen(new IPEndPoint(IPAddress.Any, 9287));
            });

            // runs scheduler on calling thread. this method will block until
            // someone calls Stop() on the scheduler.
            scheduler.Start();
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.zsocket.Dispose();
            this.zsocket = null;
        }

        #endregion
    }
}
