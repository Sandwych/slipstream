using System;
using System.Net;
using System.Threading;
using System.Dynamic;
using System.Text;
using System.IO;

using Newtonsoft.Json;

namespace ObjectServer.Client
{
    public delegate void JsonRpcCallComplete(JsonRpcResponse response, ResultHandler resultHandler);
    public delegate void ResultHandler(object returnValue);

    public class JsonRpcClient : IAsyncResult
    {
        class CallResult
        {
            public JsonRpcResponse Response { get; private set; }
            public ResultHandler ResultHandler { get; private set; }

            public CallResult(JsonRpcResponse response, ResultHandler resultHander)
            {
                this.Response = response;
                this.ResultHandler = resultHander;
            }
        }

        class RequestState
        {
            public HttpWebRequest Request;
            public HttpWebResponse Response;
            public JsonRpcClient jsonRpcClient;
            public JsonRpcRequest jsonRequest;
            public JsonRpcResponse jsonResponse;
            public ResultHandler resultHandler;

            public RequestState(HttpWebRequest req, JsonRpcClient jsonRpcClient, JsonRpcRequest jsonRequest)
            {
                Request = req;
                this.jsonRpcClient = jsonRpcClient;
                this.jsonRequest = jsonRequest;

                Response = null;
                this.jsonResponse = null;
            }
        }

        private Uri uri;
        private ManualResetEvent asyncWaitHandle = new ManualResetEvent(false);
        private SynchronizationContext syncCtx;

        private AutoResetEvent callingEvent = new AutoResetEvent(false);
        private JsonRpcCallComplete JsonRpcCallCompleteHandler;

        public JsonRpcClient(Uri uri)
        {
            this.uri = uri;
        }

        #region IAsyncResult Members

        public object AsyncState { get; private set; }
        public WaitHandle AsyncWaitHandle { get { return this.asyncWaitHandle; } }
        public bool CompletedSynchronously { get { return false; } }
        public bool IsCompleted { get; private set; }

        #endregion


        public IAsyncResult InvokeAsync(string method, object[] args, ResultHandler resultHandler)
        {
            var jreq = new JsonRpcRequest(method, args);

            //TODO 线程安全
            this.syncCtx = SynchronizationContext.Current;
            this.JsonRpcCallCompleteHandler = new JsonRpcCallComplete(this.CallCompleteHandler);
            return this.InvokeAsync(jreq, resultHandler);
        }

        public IAsyncResult InvokeAsync(JsonRpcRequest jreq, ResultHandler resultHandler)
        {
            try
            {
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(this.uri);
                httpRequest.Method = "POST";
                httpRequest.ContentType = "text/json";

                var state = new RequestState(httpRequest, this, jreq);
                state.resultHandler = resultHandler;
                httpRequest.BeginGetRequestStream(new AsyncCallback(RequestStreamResponse), state);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception while processing execute request: " + e.Message);
                throw e;
            }
            return this;
        }

        private void CallCompleteHandler(JsonRpcResponse response, ResultHandler resultHandler)
        {
            var callResult = new CallResult(response, resultHandler);
            this.syncCtx.Post(this.ReceiveReturnValue, callResult); 
        }

        private void ReceiveReturnValue(object state)
        {
            var callResult = state as CallResult;
            callResult.ResultHandler(callResult.Response.Result);
        }

        void RequestStreamResponse(IAsyncResult result)
        {
            RequestState state = (RequestState)result.AsyncState;
            using (Stream writeStream = state.Request.EndGetRequestStream(result))
            {
                state.jsonRequest.SerializeTo(writeStream);
                writeStream.Close();
            }
            state.Request.BeginGetResponse(new AsyncCallback(OnReceiveResponse), state);
        }

        void OnReceiveResponse(IAsyncResult result)
        {
            RequestState state = result.AsyncState as RequestState;
            state.Response = (HttpWebResponse)state.Request.EndGetResponse(result);

            using(var readStream = state.Response.GetResponseStream())
            {
                state.jsonResponse = JsonRpcResponse.Deserialize(readStream);
                readStream.Close();
            }

            AsyncState = state;

            if (state.jsonRpcClient.JsonRpcCallCompleteHandler != null)
            {
                state.jsonRpcClient.JsonRpcCallCompleteHandler(state.jsonResponse, state.resultHandler);
            }

            IsCompleted = true;
            asyncWaitHandle.Set();
        }



    }
}
