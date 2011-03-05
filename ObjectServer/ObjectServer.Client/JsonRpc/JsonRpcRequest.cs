using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace ObjectServer.Client
{

    [JsonObject]
    public class JsonRpcRequest 
    {
        public JsonRpcRequest(string method, params object[] args)
        {
            this.Method = method;
            this.Params = args;
            this.Id = Guid.NewGuid().ToString();
        }

        [JsonProperty("method")]
        public string Method { get; private set; }

        [JsonProperty("params")]
        public object[] Params { get; private set; }

        [JsonProperty("id")]
        public object Id { get; private set; }

        public void SerializeTo(Stream output)
        {
            using (var sw = new StreamWriter(output, Encoding.UTF8))
            {
                sw.Write(JsonConvert.SerializeObject(this, Formatting.None));
                sw.Close();
            }
        }
    }
}
