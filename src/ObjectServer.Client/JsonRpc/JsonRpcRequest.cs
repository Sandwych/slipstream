using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Diagnostics;

using Newtonsoft.Json;

namespace ObjectServer.Client
{

    [JsonObject]
    public class JsonRpcRequest : IAsyncResult
    {
        public JsonRpcRequest(JsonRpcCallCompleteCallback callCompleteProc, string method, params object[] args)
        {
            this.Method = method;
            this.Params = args;
            this.Id = Guid.NewGuid().ToString();
            this.JsonRpcCallCompleteHandler = callCompleteProc;
        }

        [JsonProperty("method")]
        public string Method { get; private set; }

        [JsonProperty("params")]
        public object[] Params { get; private set; }

        [JsonProperty("id")]
        public object Id { get; private set; }


        private ManualResetEvent asyncWaitHandle = new ManualResetEvent(false);
        private JsonRpcCallCompleteCallback JsonRpcCallCompleteHandler;

        #region IAsyncResult Members

        [JsonIgnore]
        public object AsyncState { get; private set; }

        [JsonIgnore]
        public WaitHandle AsyncWaitHandle { get { return this.asyncWaitHandle; } }

        [JsonIgnore]
        public bool CompletedSynchronously { get { return false; } }

        [JsonIgnore]
        public bool IsCompleted { get; private set; }

        #endregion

        public IAsyncResult Send(Uri uri, Action<object> resultHandler)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "text/json";

            var state = new RequestState(httpRequest, resultHandler);
            httpRequest.BeginGetRequestStream(new AsyncCallback(OnRequestReady), state);

            return this;
        }

        private void OnRequestReady(IAsyncResult result)
        {
            Debug.Assert(result.IsCompleted);

            RequestState state = (RequestState)result.AsyncState;

            using (var stream = state.HttpRequest.EndGetRequestStream(result))
            using (var sw = new StreamWriter(stream, Encoding.UTF8))
            {
                var json = JsonConvert.SerializeObject(this, Formatting.None);
                sw.Write(json);
                sw.Flush();
            }
            state.HttpRequest.BeginGetResponse(new AsyncCallback(OnResponseReady), state);
        }

        private void OnResponseReady(IAsyncResult result)
        {
            RequestState state = result.AsyncState as RequestState;
            state.HttpResponse = (HttpWebResponse)state.HttpRequest.EndGetResponse(result);

            using (var readStream = state.HttpResponse.GetResponseStream())
            {
                state.JsonResponse = JsonRpcResponse.Deserialize(readStream);
            }

            AsyncState = state;

            this.JsonRpcCallCompleteHandler(state.JsonResponse, state.ResultHandler);

            IsCompleted = true;
            asyncWaitHandle.Set();
        }

    }
}
