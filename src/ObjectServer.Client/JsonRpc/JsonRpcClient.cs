using System;
using System.Net;
using System.Threading;
using System.Dynamic;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace ObjectServer.Client
{
    public delegate void JsonRpcCallCompleteCallback(JsonRpcResponse response, Action<object, Exception> resultHandler);

    public class JsonRpcClient
    {
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
        public void BeginInvoke(string method, object[] args, Action<object> resultCallback)
        {
            this.BeginInvoke(method, args, (o, e) => resultCallback(o));
        }

        public void BeginInvoke(string method, object[] args, Action<object, Exception> resultCallback)
        {
            var jreq = new JsonRpcRequest(method, args);
            var syncCtx = SynchronizationContext.Current;
            jreq.BeginPost(this.Uri, (jrep, e) =>
            {
                syncCtx.Post(state =>
                {
                    if (jrep != null)
                    {
                        resultCallback(jrep.Result, e);
                    }
                    else
                    {
                        resultCallback(null, e);
                    }
                }, null);
            });
        }

        public void InvokeAsync(string method, object[] args, Action<object, Exception> resultCallback)
        {
            var jreq = new JsonRpcRequest(method, args);
            jreq.BeginPost(this.Uri, (jrep, e) =>
            {
                if (jrep != null)
                {
                    resultCallback(jrep.Result, e);
                }
                else
                {
                    resultCallback(null, e);
                }
            });
        }

        public Task<object> InvokeAsync(string method, object[] args)
        {
            var jreq = new JsonRpcRequest(method, args);
            var tcs = new TaskCompletionSource<object>();
            jreq.PostAsync(this.Uri).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    tcs.SetException(task.Exception);
                    return;
                }

                var error = task.Result.Error;
                if (error != null)
                {
                    var ex = new JsonRpcException("调用JSON-RPC 失败", error);
                    tcs.SetException(ex);
                    return;
                }
                tcs.SetResult(task.Result.Result);
            });
            return tcs.Task;
        }

        public Task<dynamic> InvokeDyanmicAsync(string method, object[] args)
        {
            return this.InvokeAsync(method, args);
        }

    }
}
