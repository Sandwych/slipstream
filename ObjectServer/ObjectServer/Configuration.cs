using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ObjectServer
{
    [Serializable]
    [JsonObject("objectserver-config")]
    public sealed class Configuration
    {

        /// <summary>
        /// 配置文件路径
        /// </summary>
        [JsonIgnore]
        public string ConfigurationPath { get; set; }

        [JsonProperty("db-type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Backend.DatabaseType DbType { get; set; }

        [JsonProperty("db-host")]
        public string DbHost { get; set; }

        [JsonProperty("db-port")]
        public int DbPort { get; set; }

        [JsonProperty("db-user")]
        public string DbUser { get; set; }

        [JsonProperty("db-password")]
        public string DbPassword { get; set; }

        /// <summary>
        /// 指定连接的数据库名，如果没有指定，则可以连接到用户所属的多个数据库。
        /// </summary>
        [JsonProperty("db-name")]
        public string DbName { get; set; }

        [JsonProperty("module-path", Required = Required.Always)]
        public string ModulePath { get; set; }

        [JsonProperty("debug")]
        public bool Debug { get; set; }

    }
}
