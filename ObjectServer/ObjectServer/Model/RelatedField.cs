using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace ObjectServer.Model
{

    [JsonObject("related-object")]
    [Serializable]
    public struct RelatedField
    {
        public RelatedField(long id, string name)
            : this()
        {
            this.Id = id;
            this.Name = name;
        }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }


        public static RelatedField Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<RelatedField>(json);
        }
    }
}
