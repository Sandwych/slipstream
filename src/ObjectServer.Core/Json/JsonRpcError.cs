using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace ObjectServer.Json
{
    [JsonObject("error")]
    public sealed class JsonRpcError
    {
        public JsonRpcError()
        {
            this.Name = "JSONRPCError";
        }

        public JsonRpcError(string code, string message)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentNullException("message");
            }

            this.Code = code;
            this.Message = message;
        }

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; private set; }

        [JsonProperty("code", Required = Required.Always)]
        public string Code { get; private set; }

        [JsonProperty("message", Required = Required.Always)]
        public string Message { get; private set; }


        //错误列表定义：
        public static readonly JsonRpcError ServerFatalError =
            new JsonRpcError("9999", "服务器程序发生了致命错误，请与系统管理员联系");

        public static readonly JsonRpcError ServerInternalError = 
            new JsonRpcError("0001", "服务器发生内部错误，请与系统管理员联系");

        public static readonly JsonRpcError ServerDatabaseError =
            new JsonRpcError("0002", "数据库访问出错");

        public static readonly JsonRpcError RpcArgumentError =
            new JsonRpcError("0003", "JSON-RPC 参数不正确");

        public static readonly JsonRpcError LogginError =
            new JsonRpcError("0004", "登录错误");

        public static readonly JsonRpcError AccessDeniedError =
            new JsonRpcError("0005", "权限不足，访问被禁止");

    }
}
