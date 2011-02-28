using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.IO;
using System.Text;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

using ObjectServer.Json;

namespace ObjectServer.Web
{
    public abstract class JsonRpcHandler : IHttpHandler
    {
        private Dictionary<string, MethodInfo> methods
            = new Dictionary<string, MethodInfo>();

        protected JsonRpcHandler()
        {
            this.RegisterStaticRpcMethods(typeof(JsonRpcHandler));
            var sublcassType = this.GetType();
            this.RegisterStaticRpcMethods(sublcassType);
            sublcassType = null;
        }

        public void RegisterMethod(string methodName, MethodInfo mi)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException("methodName");
            }

            if (mi == null)
            {
                throw new ArgumentNullException("mi");
            }

            lock (this)
            {
                try
                {
                    this.methods.Add(methodName, mi);
                }
                catch (ArgumentException)
                {
                    Logger.Error(() =>
                        string.Format("Duplicated method: '{0}'", methodName));
                    throw;
                }
            }
        }

        private void RegisterStaticRpcMethods(Type type)
        {
            //TODO: 不知为何 GetMethod(BindAttrs) 的总是返回0
            var methods = from mi in type.GetMethods()
                          where mi.IsPublic && mi.DeclaringType == type
                          select mi;

            foreach (var mi in methods)
            {
                var attrs = mi.GetCustomAttributes(typeof(JsonRpcMethodAttribute), false);
                if (attrs.Length == 1)
                {
                    var attr = (JsonRpcMethodAttribute)attrs[0];

                    var name = attr.Name;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = mi.Name;
                    }

                    this.RegisterMethod(name, mi);
                }
            }
        }


        #region JSON-RPC system methods

        [JsonRpcMethod(Name = "system.listMethods")]
        public string[] ListMethods()
        {
            return this.methods.Keys.ToArray();
        }


        [JsonRpcMethod(Name = "system.echo")]
        public object Echo(object value)
        {
            return value;
        }

        #endregion


        #region IHttpHandler 成员

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var httpMethod = context.Request.HttpMethod.Trim().ToUpperInvariant();

            if (httpMethod == "POST")
            {
                this.ProcessJsonRpc(
                    context.Request.InputStream, context.Response.Output);
                context.Response.ContentType = JsonRpcProtocol.JsonContentType;
            }
        }

        public void ProcessJsonRpc(Stream inputStream, TextWriter resultWriter)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException("inputStream");
            }

            if (resultWriter == null)
            {
                throw new ArgumentNullException("resultWriter");
            }

            var jreq = (Dictionary<string, object>)PlainJsonConvert.Deserialize(inputStream);

            //执行调用
            var id = jreq[JsonRpcProtocol.Id];
            var methodName = (string)jreq[JsonRpcProtocol.Method];
            var method = this.methods[methodName];
            string error = null;
            object result = null;

            var args = (object[])jreq[JsonRpcProtocol.Params];

            //TODO: 处理安全问题及日志异常等
            try
            {
                result = method.Invoke(this, args);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            var jresponse = new JsonRpcResponse()
            {
                Id = id,
                Error = error,
                Result = result
            };

            var js = new JsonSerializer();
            js.Serialize(resultWriter, jresponse);
        }

        #endregion
    }
}
