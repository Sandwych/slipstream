using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

using Newtonsoft.Json;

using Malt.Threading;
using Malt.Json;

namespace Malt.Json
{

    [JsonObject]
    public class JsonRpcRequest
    {
        public event JsonRequestCompletedHandler JsonRequestCompleted;

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

        public void PostAsync(Uri uri, object userState = null)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            this.Post(uri, (result, error) =>
            {
                if (this.JsonRequestCompleted != null)
                {
                    var args = new JsonRequestCompletedEventArgs(result, error, userState);
                    this.JsonRequestCompleted(this, args);
                }
            });
        }

        public void Post(Uri uri, Action<JsonRpcResponse, Exception> resultCallback)
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

#if TPL

        public Task<JsonRpcResponse> PostTaskAsync(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }

            var webReq = WebRequest.CreateHttp(uri);
            webReq.Method = "POST";
            webReq.ContentType = "text/json";

            return webReq.GetRequestStreamAsync().ContinueWith(t =>
            {
                using (var reqStream = t.Result)
                {
                    this.SerializeTo(reqStream);
                }
            }).ContinueWith(t2 =>
            {
                return webReq.GetReponseAsync().ContinueWith<JsonRpcResponse>(ca2 =>
                 {
                     var webRep = ca2.Result;
                     using (var repStream = webRep.GetResponseStream())
                     {
                         var jsonRep = JsonRpcResponse.Deserialize(repStream);
                         return jsonRep;
                     }
                 }, TaskContinuationOptions.AttachedToParent).Result;
            });
        }
#endif //TPL

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

            var reqStream = webReq.EndGetRequestStream(ae.DequeueAsyncResult());
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
