using System;
using System.Net;
using System.Threading;
using System.Dynamic;
using System.Text;
using System.IO;

using Newtonsoft.Json;

namespace ObjectServer.Client
{
    public delegate void JsonRpcCallComplete(JsonRpcResponse response, ResultCallbackHandler resultHandler);
    public delegate void ResultCallbackHandler(object returnValue);

    public class JsonRpcClient 
    {
        class CallResult
        {
            public JsonRpcResponse Response { get; private set; }
            public ResultCallbackHandler ResultCallback { get; private set; }

            public CallResult(JsonRpcResponse response, ResultCallbackHandler resultCallback)
            {
                this.Response = response;
                this.ResultCallback = resultCallback;
            }
        }

        private SynchronizationContext syncCtx;

        public JsonRpcClient(Uri uri)
        {
            this.Uri = uri;
        }

        public Uri Uri { get; private set; }

        /// <summary>
        /// 执行异步调用，resultCallback 将在不确定的线程中执行
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <param name="resultCallback"></param>
        /// <returns></returns>
        public IAsyncResult InvokeAsync(string method, object[] args, ResultCallbackHandler resultCallback)
        {
            var jreq = new JsonRpcRequest(this.SyncCallCompleteHandler, method, args);
            return jreq.Send(this.Uri, resultCallback);
        }

        /// <summary>
        /// 执行同步调用，resultCallback 将在调用此方法的同一线程中执行，推荐使用此方法更新 UI
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <param name="resultCallback"></param>
        /// <returns></returns>
        public IAsyncResult InvokeSync(string method, object[] args, ResultCallbackHandler resultCallback)
        {
            var jreq = new JsonRpcRequest(this.SyncCallCompleteHandler, method, args);
            this.syncCtx = SynchronizationContext.Current;
            return jreq.Send(this.Uri, resultCallback);
        }

        private void SyncCallCompleteHandler(JsonRpcResponse response, ResultCallbackHandler resultCallback)
        {
            var callResult = new CallResult(response, resultCallback);
            this.syncCtx.Post(this.ReceiveReturnValue, callResult);
        }

        private void AsyncCallCompleteHandler(JsonRpcResponse response, ResultCallbackHandler resultCallback)
        {
            var callResult = new CallResult(response, resultCallback);
            this.ReceiveReturnValue(callResult);
        }

        private void ReceiveReturnValue(object state)
        {
            var callResult = state as CallResult;
            callResult.ResultCallback(callResult.Response.Result);
        }

     
    }
}
