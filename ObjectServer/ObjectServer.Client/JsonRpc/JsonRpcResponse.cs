using System;
using System.Net;
using System.Text;
using System.IO;

using Newtonsoft.Json;

namespace ObjectServer.Client
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

        public static JsonRpcResponse Deserialize(Stream input)
        {
            using (var reader = new StreamReader(input, Encoding.UTF8))
            {
                var json = reader.ReadToEnd();
                reader.Close();
                return JsonConvert.DeserializeObject<JsonRpcResponse>(json);
            }
        }
    }
}
