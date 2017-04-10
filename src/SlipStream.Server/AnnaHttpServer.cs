using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Threading;
using Anna;

using SlipStream;
using System.Reactive.Concurrency;

namespace SlipStream.Server
{

    /// <summary>
    /// HTTP RPC 接口服务器
    /// </summary>
    public sealed class HttpServer : IDisposable
    {
        private const string JsonRpcPath = "/jsonrpc";
        private const string CrossDomainPath = "/crossdomain.xml";
        private readonly IDictionary<string, string> DefaultJsonRpcHeader = new Dictionary<string, string>()
        {
            { "Content-Type", "text/javascript" }
        };

        public const string CrossDomainText =
            "<?xml version=\"1.0\"?>" +
            "<cross-domain-policy>" +
            "<allow-access-from domain=\"*\" />" +
            "</cross-domain-policy>";

        private readonly string _httpHostUri;

        private bool disposed = false;

        public HttpServer(string httpUrl)
        {
            if (string.IsNullOrEmpty(httpUrl))
            {
                throw new ArgumentNullException("httpUrl");
            }

            LoggerProvider.EnvironmentLogger.Info("Starting HTTP Server...");


            if (!httpUrl.EndsWith("/"))
            {
                httpUrl += '/';
            }

            this._httpHostUri = httpUrl;
            LoggerProvider.EnvironmentLogger.Info(String.Format("HTTP Listen: [{0}]", httpUrl));

        }

        ~HttpServer()
        {
            this.Dispose(false);
        }

        public void Start(Action waitingBlocker)
        {
            LoggerProvider.EnvironmentLogger.Info(
                String.Format("Starting the HTTP Server [{0}]...", this._httpHostUri));

            using (var eventLoop = new EventLoopScheduler())
            using (var httpd = new Anna.HttpServer(this._httpHostUri, eventLoop))
            {
                this.RegisterRequestHandlers(httpd);
                waitingBlocker();
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
                context.Response(repData, 200).Send();
            });
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

            var jresponse = ServiceDispatcher.InvokeJsonRpc(reqData);

            LoggerProvider.RpcLogger.Debug(() =>
            {
                var repStr = Encoding.UTF8.GetString(jresponse);
                return string.Format("JSON Response=[{0}]", repStr);
            });

            return jresponse;
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
