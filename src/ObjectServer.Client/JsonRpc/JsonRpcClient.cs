using System;
using System.Net;
using System.Threading;
using System.Dynamic;
using System.Text;
using System.IO;

using Newtonsoft.Json;

namespace ObjectServer.Client
{
    public delegate void JsonRpcCallCompleteCallback(JsonRpcResponse response, Action<object> resultHandler);

    public class JsonRpcClient
    {
        class CallResult
        {
            public JsonRpcResponse Response { get; private set; }
            public Action<object> ResultCallback { get; private set; }

            public CallResult(JsonRpcResponse response, Action<object> resultCallback)
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
        /// 执行同步调用，resultCallback 将在调用此方法的同一线程中执行，推荐使用此方法更新 UI
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <param name="resultCallback"></param>
        /// <returns></returns>
        public IAsyncResult SyncInvoke(string method, object[] args, Action<object> resultCallback)
        {
            var jreq = new JsonRpcRequest(this.SyncCallCompleteCallback, method, args);
            this.syncCtx = SynchronizationContext.Current;
            return jreq.Send(this.Uri, resultCallback);
        }

        public IAsyncResult AsyncInvoke(string method, object[] args, Action<object> resultCallback)
        {
            var jreq = new JsonRpcRequest(this.AsyncCallCompleteCallback, method, args);
            return jreq.Send(this.Uri, resultCallback);
        }

        private void SyncCallCompleteCallback(JsonRpcResponse response, Action<object> resultCallback)
        {
            var callResult = new CallResult(response, resultCallback);
            this.syncCtx.Post(this.ReceiveReturnValueCallback, callResult);
        }

        private void AsyncCallCompleteCallback(JsonRpcResponse response, Action<object> resultCallback)
        {
            var callResult = new CallResult(response, resultCallback);
            this.ReceiveReturnValueCallback(callResult);
        }

        private void ReceiveReturnValueCallback(object state)
        {
            var callResult = state as CallResult;
            if (callResult.Response.Error != null)
            {
                throw new Exception(callResult.Response.Error.ToString());
            }

            callResult.ResultCallback(callResult.Response.Result);
        }


    }
}
