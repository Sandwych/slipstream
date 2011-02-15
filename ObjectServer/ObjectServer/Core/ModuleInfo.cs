using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace ObjectServer.Core
{
    /// <summary>
    /// DTO class for Module
    /// </summary>
    [Serializable]
    [JsonObject("module")]
    public sealed class ModuleInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("source-files")]
        public string[] SourceFiles { get; set; }

        [JsonProperty("data-files")]
        public string[] DataFiles { get; set; }

    }
}
