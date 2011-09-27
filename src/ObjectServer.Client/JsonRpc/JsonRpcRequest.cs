using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using Newtonsoft.Json;

using ObjectServer.Threading;

namespace ObjectServer.Client
{

    [JsonObject]
    public class JsonRpcRequest
    {
        public JsonRpcRequest(string method, params object[] args)
        {
            this.Method = method;
            this.Params = args;
            this.Id = Guid.NewGuid().ToString();
        }

        [JsonProperty("method")]
        public string Method { get; private set; }

        [JsonProperty("params")]
        public object[] Params { get; private set; }

        [JsonProperty("id")]
        public object Id { get; private set; }

        public void PostAsync(Uri uri, Action<JsonRpcResponse, Exception> resultCallback)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            if (resultCallback == null)
            {
                throw new ArgumentNullException("resultCallback");
            }

            try
            {
                var ae = new AsyncEnumerator();
                ae.Execute(this.GetPostEnumerator(uri, ae, resultCallback));
            }
            catch (Exception ex)
            {
                resultCallback(null, ex);
            }
        }

        private IEnumerator<int> GetPostEnumerator(
            Uri uri, AsyncEnumerator ae, Action<JsonRpcResponse, Exception> resultCallback)
        {
            var webReq = (HttpWebRequest)WebRequest.Create(uri);
            webReq.Method = "POST";
            webReq.ContentType = "text/json";
            webReq.BeginGetRequestStream(ae.End(), null);

            yield return 1;

            using (var reqStream = webReq.EndGetRequestStream(ae.DequeueAsyncResult()))
            using (var sw = new StreamWriter(reqStream, Encoding.UTF8))
            {
                var json = JsonConvert.SerializeObject(this, Formatting.None);
                sw.Write(json);
                sw.Flush();
            }

            webReq.BeginGetResponse(ae.End(), null);

            yield return 1;

            try
            {
                using (var webRep = webReq.EndGetResponse(ae.DequeueAsyncResult()))
                using (var repStream = webRep.GetResponseStream())
                {
                    var jsonRep = JsonRpcResponse.Deserialize(repStream);
                    resultCallback(jsonRep, null);
                }
            }
            catch (Exception ex)
            {
                resultCallback(null, ex);
                yield break;
            }
        }

    }
}
