using System;
using System.Net;
using System.Threading;
using System.Dynamic;
using System.Text;
using System.IO;

using Newtonsoft.Json;

namespace ObjectServer.Client
{
    public delegate void JsonRpcCallCompleteCallback(JsonRpcResponse response, Action<object, Exception> resultHandler);

    public class JsonRpcClient
    {
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
        public void Invoke(string method, object[] args, Action<object> resultCallback)
        {
            var jreq = new JsonRpcRequest(method, args);
            this.syncCtx = SynchronizationContext.Current;
            jreq.PostAsync(this.Uri, (jrep, e) =>
            {
                this.syncCtx.Post(state =>
                {
                    resultCallback(jrep.Result);
                }, null);
            });
        }

        public void InvokeAsync(string method, object[] args, Action<object> resultCallback)
        {
            var jreq = new JsonRpcRequest(method, args);
            jreq.PostAsync(this.Uri, (jrep, e) =>
            {
                resultCallback(jrep.Result);
            });
        }


    }
}
