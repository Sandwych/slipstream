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

    public class JsonRpcClient 
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

        private Uri uri;
        private SynchronizationContext syncCtx;

        public JsonRpcClient(Uri uri)
        {
            this.uri = uri;
        }

        public IAsyncResult InvokeAsync(string method, object[] args, ResultHandler resultHandler)
        {
            var jreq = new JsonRpcRequest(this.CallCompleteHandler, method, args);
            this.syncCtx = SynchronizationContext.Current;
            return jreq.Send(this.uri, resultHandler);
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

     
    }
}
