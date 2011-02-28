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
                this.methods.Add(methodName, mi);
            }
        }

        private void RegisterStaticRpcMethods(Type type)
        {
            var methods = type.GetMethods();
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

        private void RegisterDynamicRpcMethod(MethodInfo mi)
        {
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
