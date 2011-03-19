using System;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;

using ObjectServer.Json;

namespace ObjectServer.Client
{
    [JsonObject]
    public sealed class JsonRpcResponse
    {
        public JsonRpcResponse()
        {
        }

        public JsonRpcResponse(IDictionary<string, object> propertyBag)
        {
            if(propertyBag.ContainsKey("error"))
            {
            this.Error = propertyBag["error"];
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
        public object Error { get; set; }

        [JsonProperty("id", Required = Required.Always)]
        public object Id { get; set; }

        public static JsonRpcResponse Deserialize(Stream input)
        {
            using (var reader = new StreamReader(input, Encoding.UTF8))
            {           
                var propBag = (Dictionary<string, object>)PlainJsonConvert.Deserialize(reader);
                reader.Close();
                return new JsonRpcResponse(propBag);
            }
        }
    }
}
