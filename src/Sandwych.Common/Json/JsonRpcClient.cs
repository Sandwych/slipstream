using System;
using System.Net;
using System.Threading;
using System.Dynamic;
using System.Text;
using System.IO;

using Newtonsoft.Json;

namespace Sandwych.Json
{
    public delegate void JsonRpcCallCompleteCallback(JsonRpcResponse response, Action<object, Exception> resultHandler);

    public class JsonRpcClient
    {
        public event JsonRpcCompletedHandler JsonRpcCompleted;

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
        /*
        public void BeginInvoke(string method, object[] args, Action<object, Exception> resultCallback)
        {
            this.BeginInvoke(method, args, (o, e) => resultCallback(o, e));
        }
        */

        /*
        public void BeginInvoke(string method, object[] args, Action<object, Exception> resultCallback)
        {
            var jreq = new JsonRpcRequest(method, args);
            var syncCtx = SynchronizationContext.Current;
            jreq.BeginPost(this.Uri, (jrep, e) =>
            {
                syncCtx.Post(delegate
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
        */
        public void InvokeAsync(string method, object[] args, object userState = null)
        {
            var jreq = new JsonRpcRequest(method, args);
            jreq.JsonRequestCompleted += this.OnJsonRequestCompleted;
            jreq.PostAsync(this.Uri, userState);
        }

        private void OnJsonRequestCompleted(object sender, JsonRequestCompletedEventArgs args)
        {
            if (this.JsonRpcCompleted != null)
            {
                var rpcArgs = new JsonRpcCompletedEventArgs(args.Result, args.Error, args.UserState);
                this.JsonRpcCompleted(this, rpcArgs);
            }
        }

        public void Invoke(string method, object[] args, Action<object, Exception> resultCallback)
        {
            if (string.IsNullOrEmpty(method))
            {
                throw new ArgumentNullException("method");
            }

            var jreq = new JsonRpcRequest(method, args);
            var syncCtx = SynchronizationContext.Current;
            jreq.Post(this.Uri, (jrep, postError) =>
            {
                syncCtx.Post(delegate
                {
                    if (postError != null)
                    {
                        resultCallback(null, postError);
                        return;
                    }
                    else
                    {
                        if (jrep.Error == null)
                        {
                            resultCallback(jrep.Result, null);
                        }
                        else
                        {
                            var msg = String.Format("Failed to invoke JSON-RPC: {0}", jrep.Error);
                            var error = new JsonRpcException(msg, jrep.Error);
                            resultCallback(null, error);
                        }
                    }
                }, null);
            });
        }

#if TPL
        public Task<object> InvokeTaskAsync(string method, object[] args)
        {
            var jreq = new JsonRpcRequest(method, args);
            var mainTask = jreq.PostTaskAsync(this.Uri).ContinueWith<object>(task =>
            {
                var error = task.Result.Error;
                if (error != null)
                {
                    var ex = new JsonRpcException("调用JSON-RPC 失败", error);
                    throw ex;
                }
                else
                {
                    return task.Result;
                }
            });
            return mainTask;
        }

        public Task<dynamic> InvokeDyanmicAsync(string method, object[] args)
        {
            return this.InvokeTaskAsync(method, args);
        }
#endif//TPL

    }
}
