using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;

using Kayak;
using Kayak.Http;

namespace ObjectServer.Http
{

    /// <summary>
    /// HTTP RPC 接口服务器
    /// </summary>
    public sealed class HttpServer : IDisposable
    {

        private readonly string rpcReqUrl;
        private readonly IPEndPoint httpEndPoint;
        private ZMQ.Socket zsocket;

        public HttpServer(string rpcHostUrl, int listenPort)
        {
            if (string.IsNullOrEmpty(rpcHostUrl))
            {
                throw new ArgumentNullException("rpcHostUrl");
            }

            if (listenPort <= 0 || listenPort >= UInt16.MaxValue)
            {
                throw new ArgumentOutOfRangeException("listenPort");
            }

            this.httpEndPoint = new IPEndPoint(IPAddress.Any, listenPort);

            //TODO: 看一下为什么只能用 TCP 的
            this.rpcReqUrl = rpcHostUrl;
            this.zsocket = new ZMQ.Socket(ZMQ.SocketType.REQ);
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
