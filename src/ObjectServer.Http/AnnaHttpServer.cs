using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Threading;

using ObjectServer;

namespace ObjectServer.Http
{

    /// <summary>
    /// HTTP RPC 接口服务器
    /// </summary>
    public sealed class AnnaHttpServer : IDisposable
    {
        private const string JsonRpcPath = "/jsonrpc";
        private const string CrossDomainPath = "/crossdomain.xml";

        public const string CrossDomainText =
            "<?xml version=\"1.0\"?>" +
            "<cross-domain-policy>" +
            "<allow-access-from domain=\"*\" />" +
            "</cross-domain-policy>";

        private readonly ZMQ.Socket commandSocket = new ZMQ.Socket(ZMQ.SocketType.SUB);
        private readonly ZMQ.Socket senderSocket = new ZMQ.Socket(ZMQ.SocketType.REQ);
        private readonly string rpcReqUrl;
        private readonly string httpHostUrl;

        private bool disposed = false;

        public AnnaHttpServer(string controllerUrl, string rpcHostUrl, string httpUrl)
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

            if (string.IsNullOrEmpty(httpUrl))
            {
                throw new ArgumentNullException("httpUrl");
            }

            commandSocket.Connect(controllerUrl);
            commandSocket.Subscribe("STOP", Encoding.UTF8);

            if (!httpUrl.EndsWith("/"))
            {
                httpUrl += '/';
            }

            this.httpHostUrl = httpUrl;
            LoggerProvider.EnvironmentLogger.Info(String.Format("HTTP Listen: [{0}]", httpUrl));

            this.rpcReqUrl = rpcHostUrl;
            LoggerProvider.EnvironmentLogger.Info("Connecting to MQ: [" + this.rpcReqUrl + "]");
        }

        ~AnnaHttpServer()
        {
            this.Dispose(false);
        }

        public void Start()
        {
            Debug.Assert(this.senderSocket != null);

            this.senderSocket.Connect(this.rpcReqUrl);

            LoggerProvider.EnvironmentLogger.Info(
                String.Format("Starting the HTTP Server [{0}]...", this.httpHostUrl));

            using (var httpd = new Anna.HttpServer(this.httpHostUrl))
            {
                this.RegisterRequestHandlers(httpd);

                this.WaitForStopCommand();
            }

            LoggerProvider.EnvironmentLogger.Info("The HTTP server is stopped.");
        }

        private void RegisterRequestHandlers(Anna.HttpServer httpd)
        {
            httpd.GET(CrossDomainPath).Subscribe(context =>
            {
                context.Respond(CrossDomainText);
            });

            httpd.POST(JsonRpcPath).Subscribe(context =>
            {
                var repData = this.HandleJsonRpcRequest(context.Request);
                var response = new Anna.Responses.BinaryResponse(repData);
                response.Headers["Content-Type"] = "text/javascript";
                context.Respond(response);
            });
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
                    break;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="System.Net.WebException" />
        private byte[] HandleJsonRpcRequest(Anna.Request.Request req)
        {
            //TODO 可修改的请求限制大小

            const int MaxRequestSize = 1024 * 1024 * 4;

            if (req.ContentLength > MaxRequestSize)
            {
                throw new System.Net.WebException("Too large request");
            }

            byte[] reqData = new byte[req.ContentLength];
            using (var inStream = req.InputStream)
            {
                var n = inStream.Read(reqData, 0, reqData.Length);
                Debug.Assert(n == reqData.Length);
            }

            LoggerProvider.RpcLogger.Debug(() =>
            {
                var reqStr = Encoding.UTF8.GetString(reqData);
                return string.Format("JSON Request=[{0}]", reqStr);
            });

            var jresponse = this.CallJsonRpc(reqData);

            LoggerProvider.RpcLogger.Debug(() =>
            {
                var repStr = Encoding.UTF8.GetString(jresponse);
                return string.Format("JSON Response=[{0}]", repStr);
            });

            return jresponse;
        }

        private byte[] CallJsonRpc(byte[] reqData)
        {
            Debug.Assert(reqData != null);

            var startTime = Stopwatch.GetTimestamp();

            this.senderSocket.Send(reqData);
            var rep = this.senderSocket.Recv();

            var endTime = Stopwatch.GetTimestamp();
            var costTime = endTime - startTime;
            LoggerProvider.RpcLogger.Debug(
                () => String.Format("ZMQ RPC cost time: [{0:N0}ms]", costTime * 1000 / Stopwatch.Frequency));

            return rep;
        }

        #region IDisposable Members

        private void Dispose(bool isDisposing)
        {
            if (!this.disposed)
            {

                LoggerProvider.EnvironmentLogger.Debug("Disposing HTTP Server...");
                if (isDisposing)
                {
                    //释放托管资源
                }

                //释放非托管资源

                this.senderSocket.Dispose();
                this.commandSocket.Dispose();

                this.disposed = true;
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
