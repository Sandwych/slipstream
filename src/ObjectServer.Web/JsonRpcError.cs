using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json;

namespace ObjectServer.Web
{

    [Serializable]
    [JsonObject]
    public sealed class JsonRpcError
    {
        [JsonProperty("errorCode", Required = Required.Always)]
        public int ErrorCode { get; set; }

        [JsonProperty("message", Required = Required.Always)]
        public string Message { get; set; }
    }
}
