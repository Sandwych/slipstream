using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;

namespace ObjectServer.Module
{
    /// <summary>
    /// DTO class for Module
    /// </summary>
    [Serializable]
    [JsonObject("module")]
    public sealed class ModuleInfo
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("label", Required = Required.Default)]
        public string Label { get; set; }

        [JsonProperty("description", Required = Required.Default)]
        public string Description { get; set; }

        [JsonProperty("source-files")]
        public string[] SourceFiles { get; set; }

        [JsonProperty("data-files")]
        public string[] DataFiles { get; set; }
    }
}
