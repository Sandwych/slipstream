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
        private readonly ZMQ.Socket sender;

        public const string CrossDomainText =
            "<?xml version=\"1.0\"?>" +
            "<cross-domain-policy>" +
            "<allow-access-from domain=\"*\" />" +
            "</cross-domain-policy>";

        public static readonly byte[] CrossDomainContent = 
            Encoding.UTF8.GetBytes(CrossDomainText);
        public static readonly string CrossDomainContentLength = CrossDomainContent.Length.ToString();

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
            HttpResponseHead headers;
            IDataProducer body = null;

            if (request.Uri == "/crossdomain.xml")
            {
                headers = new HttpResponseHead()
                {
                    Status = "200 OK",
                    Headers = new Dictionary<string, string>() 
                    {
                        { "Content-Type", "text/xml" },
                        { "Content-Length", CrossDomainContentLength },
                    }
                };
                body = new BufferedProducer(CrossDomainContent);
                response.OnResponse(headers, body);
            }
            else if (request.Uri == "/jsonrpc")
            {
                requestBody.Connect(new BufferedConsumer(bufferedBody =>
                {
                    Logger.Debug(() =>
                    {
                        var reqStr = Encoding.UTF8.GetString(bufferedBody);
                        return string.Format("客户端请求的 JSON=[{0}]", reqStr);
                    });

                    var jresponse = this.CallJsonRpc(bufferedBody);

                    Logger.Debug(() =>
                    {
                        var repStr = Encoding.UTF8.GetString(jresponse);
                        return string.Format("RPC 返回的 JSON=[{0}]", repStr);
                    });

                    headers = new HttpResponseHead()
                    {
                        Status = "200 OK",
                        Headers = new Dictionary<string, string>() 
                        {
                            { "Content-Type", "text/javascript" },
                            { "Content-Length", jresponse.Length.ToString() },
                            { "Connection", "close" }
                        }
                    };
                    response.OnResponse(headers, new BufferedProducer(jresponse));
                },
                error =>
                {
                    // XXX
                    // uh oh, what happens?
                }));
            }
            else
            {
                var responseBody =
                    "The resource you requested ('" + request.Uri + "') could not be found.";
                headers = new HttpResponseHead()
                {
                    Status = "404 Not Found",
                    Headers = new Dictionary<string, string>()
                    {
                        { "Content-Type", "text/plain" },
                        { "Content-Length", responseBody.Length.ToString() }
                    }
                };
                var body404 = new BufferedProducer(responseBody);

                response.OnResponse(headers, body404);
            }

        }

        private byte[] CallJsonRpc(byte[] jrequest)
        {
            //TODO Dispose
            this.sender.Send(jrequest);
            var rep = this.sender.Recv();
            return rep;
        }
    }
}
