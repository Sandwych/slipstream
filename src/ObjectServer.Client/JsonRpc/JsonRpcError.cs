using System;
using System.Collections.Generic;

using Newtonsoft.Json;
namespace ObjectServer.Client
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

        public JsonRpcError(string code, string message, object data = null)
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

        [JsonProperty("data", Required = Required.Default)]
        public object Data { get; private set; }

        [JsonProperty("code", Required = Required.Always)]
        public string Code { get; private set; }

        [JsonProperty("message", Required = Required.Always)]
        public string Message { get; private set; }

        public override string ToString()
        {
            return string.Format("[{0}]{1}", this.Code, this.Message);
        }
    }
}
