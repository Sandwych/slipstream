using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json;

namespace ObjectServer.Web
{

    [JsonObject]
    public sealed class JsonRpcRequest
    {
        [JsonProperty("method", Required = Required.Always)]
        public string Method { get; set; }

        [JsonProperty("params", Required = Required.Always)]
        public object[] Params { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public object Id { get; set; }
    }
}
