using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;

using ObjectServer.Threading;
using ObjectServer.Client.JsonRpc;

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

        public void BeginPost(Uri uri, Action<JsonRpcResponse, Exception> resultCallback)
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
                ae.BeginExecute(this.GetPostEnumerator(uri, ae, resultCallback), ar =>
                {
                    ae.EndExecute(ar);
                });
            }
            catch (System.Security.SecurityException ex)
            {
                resultCallback(null, ex);
            }
        }

        public Task<JsonRpcResponse> PostAsync(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var webReq = WebRequest.CreateHttp(uri);
            webReq.Method = "POST";
            webReq.ContentType = "text/json";

            var tcs = new TaskCompletionSource<JsonRpcResponse>();
            webReq.GetRequestStreamAsync().ContinueWith(t =>
           {
               var reqStream = t.Result;
               this.SerializeTo(reqStream);

               webReq.GetReponseAsync().ContinueWith(ca2 =>
               {
                   try
                   {
                       using (var repStream = ca2.Result.GetResponseStream())
                       {
                           var jsonRep = JsonRpcResponse.Deserialize(repStream);
                           tcs.SetResult(jsonRep);
                       }
                   }
                   catch (Exception ex) //TODO 特化异常
                   {
                       tcs.SetException(ex);
                   }
               });
           });

            return tcs.Task;
        }

        private void SerializeTo(Stream reqStream)
        {
            using (var sw = new StreamWriter(reqStream, Encoding.UTF8))
            {
                var json = JsonConvert.SerializeObject(this, Formatting.None);
                sw.Write(json);
                sw.Flush();
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
