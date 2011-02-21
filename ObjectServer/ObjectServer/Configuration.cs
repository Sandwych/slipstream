using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using ObjectServer.Utility;

namespace ObjectServer
{
    [Serializable]
    [JsonObject("objectserver-config")]
    public sealed class Configuration
    {
        public Configuration()
        {
            this.LogPath = null;
            this.LogLevel = "info";
        }


        private string rootPassword;

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

        [JsonProperty("root-password")]
        public string RootPassword
        {
            get { return this.rootPassword; }
            set
            {
                this.rootPassword = value;
                this.RootPasswordHash = value.ToSha1();
            }
        }

        [JsonProperty("log-path")]
        public string LogPath { get; set; }

        [JsonProperty("log-level")]
        public string LogLevel { get; set; }

        [JsonIgnore]
        public string RootPasswordHash { get; private set; }

    }
}
