using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Newtonsoft.Json;

namespace ObjectServer.Web
{
    [JsonObject]
    public sealed class JsonRpcResponse
    {

        [JsonProperty("result", Required = Required.AllowNull)]
        public object Result { get; set; }

        [JsonProperty("error", Required = Required.AllowNull)]
        public object Error { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public object Id { get; set; }
    }
}
