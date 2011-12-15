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
        }

        public JsonRpcError(IDictionary<string, object> propBag)
        {
            if (propBag == null)
            {
                throw new ArgumentNullException("propBag");
            }

            if (propBag.ContainsKey("data"))
            {
                this.Data = propBag["data"];
            }

            if (propBag.ContainsKey("message"))
            {
                this.Message = propBag["message"] as string;
            }

            if (propBag.ContainsKey("code"))
            {
                this.Code = propBag["code"] as string;
            }

        }

        public JsonRpcError(string code, string message, object data)
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
            this.Data = data;
        }

        public JsonRpcError(string code, string message)
            : this(code, message, null)
        {
        }

        [JsonProperty("data", Required = Required.Default)]
        public object Data { get; private set; }

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

        public static readonly JsonRpcError SecurityError =
            new JsonRpcError("0004", "安全性错误");

        public static readonly JsonRpcError AccessDeniedError =
            new JsonRpcError("0005", "权限不足，访问被禁止");

        public static readonly JsonRpcError ValidationError =
            new JsonRpcError("0006", "验证错误");

        public static readonly JsonRpcError ResourceNotFound =
            new JsonRpcError("0007", "无法找到指定的资源");

        public static readonly JsonRpcError BadData =
            new JsonRpcError("0008", "系统数据异常，原因可能是数据库中包含无效数据");

        public static readonly JsonRpcError DBError =
            new JsonRpcError("0009", "数据库操作异常，请与系统管理员联系");
    }
}
