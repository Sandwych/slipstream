using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


using Kayak;
using Kayak.Http;

namespace ObjectServer.Http
{
    internal class RequestDelegate : IHttpRequestDelegate
    {
        private const string JsonRpcPath = "/jsonrpc";

        private readonly ZMQ.Socket sender;

        public const string CrossDomainText =
            "<?xml version=\"1.0\"?>" +
            "<cross-domain-policy>" +
            "<allow-access-from domain=\"*\" />" +
            "</cross-domain-policy>";

        public static readonly byte[] CrossDomainContent =
            Encoding.UTF8.GetBytes(CrossDomainText);
        public static readonly string CrossDomainContentLength =
            CrossDomainContent.Length.ToString();

        private static readonly HttpResponseHead CrossDomainResponseHead =
            new HttpResponseHead()
        {
            Status = "200 OK",
            Headers = new Dictionary<string, string>()
            {
                { "Content-Type", "text/xml" },
                { "Content-Length", CrossDomainContentLength },
            },
        };

        private static readonly IDataProducer CrossDoaminResponseBody =
            new BufferedProducer(CrossDomainContent);


        public RequestDelegate(ZMQ.Socket zsocket)
        {
            if (zsocket == null)
            {
                throw new ArgumentNullException("zsocket");
            }
            this.sender = zsocket;
        }

        public void OnRequest(HttpRequestHead request, IDataProducer requestBody,
            IHttpResponseDelegate response)
        {
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }

            HttpResponseHead head;

            if (request.Uri == "/crossdomain.xml")
            {
                response.OnResponse(CrossDomainResponseHead, CrossDoaminResponseBody);
            }
            else if (request.Uri == JsonRpcPath)
            {
                head = this.HandleJsonRpcRequest(requestBody, response);
            }
            else
            {
                head = HandleUnknownRequest(ref request, response);
            }

        }

        private static HttpResponseHead HandleUnknownRequest(ref HttpRequestHead request, IHttpResponseDelegate response)
        {
            HttpResponseHead head;
            var responseBody =
                "The resource you requested ('" + request.Uri + "') could not be found.";
            head = new HttpResponseHead()
            {
                Status = "404 Not Found",
                Headers = new Dictionary<string, string>()
                    {
                        { "Content-Type", "text/plain" },
                        { "Content-Length", responseBody.Length.ToString() }
                    }
            };
            var body404 = new BufferedProducer(responseBody);

            response.OnResponse(head, body404);
            return head;
        }

        private HttpResponseHead HandleJsonRpcRequest(IDataProducer requestBody, IHttpResponseDelegate response)
        {
            HttpResponseHead head = new HttpResponseHead();
            requestBody.Connect(new BufferedConsumer(bufferedBody =>
            {
                LoggerProvider.RpcLogger.Debug(() =>
                {
                    var reqStr = Encoding.UTF8.GetString(bufferedBody);
                    return string.Format("客户端请求的 JSON=[{0}]", reqStr);
                });

                var jresponse = this.CallJsonRpc(bufferedBody);

                LoggerProvider.RpcLogger.Debug(() =>
                {
                    var repStr = Encoding.UTF8.GetString(jresponse);
                    return string.Format("RPC 返回的 JSON=[{0}]", repStr);
                });

                head.Status = "200 OK";
                head.Headers = new Dictionary<string, string>() 
                {
                    { "Content-Type", "text/javascript" },
                    { "Content-Length", jresponse.Length.ToString() },
                    { "Connection", "close" }
                };

                response.OnResponse(head, new BufferedProducer(jresponse));
            },
            error =>
            {
                LoggerProvider.EnvironmentLogger.Error(error);
                // XXX
                // uh oh, what happens?
            }));

            return head;
        }

        private byte[] CallJsonRpc(byte[] jrequest)
        {
            Debug.Assert(jrequest != null);

            var startTime = Stopwatch.GetTimestamp();

            this.sender.Send(jrequest);
            var rep = this.sender.Recv();

            var endTime = Stopwatch.GetTimestamp();
            var costTime = endTime - startTime;

            LoggerProvider.RpcLogger.Debug(
                () => String.Format("ZMQ RPC cost time: [{0:N0}ms]", costTime * 1000 / Stopwatch.Frequency));

            return rep;
        }
    }
}
