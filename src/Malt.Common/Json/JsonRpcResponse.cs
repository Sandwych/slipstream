using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Malt.Json
{
    [JsonObject]
    public sealed class JsonRpcResponse
    {
        public JsonRpcResponse()
        {
        }

        public JsonRpcResponse(IDictionary<string, object> propertyBag)
        {
            object error = null;
            if (propertyBag.TryGetValue("error", out error) && error != null)
            {
                this.Error = new JsonRpcError(propertyBag["error"] as IDictionary<string, object>);
            }

            if (propertyBag.ContainsKey("result"))
            {
                this.Result = propertyBag["result"];
            }

            if (propertyBag.ContainsKey("id"))
            {
                this.Id = propertyBag["id"];
            }
        }

        [JsonProperty("result", Required = Required.AllowNull)]
        public object Result { get; set; }

        [JsonProperty("error", Required = Required.AllowNull)]
        public JsonRpcError Error { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public object Id { get; set; }

        public static JsonRpcResponse Deserialize(Stream input)
        {
            using (var reader = new StreamReader(input, Encoding.UTF8))
            {
                var propBag = (Dictionary<string, object>)PlainJsonConvert.Parse(reader);
                return new JsonRpcResponse(propBag);
            }
        }
    }
}
