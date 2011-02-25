using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using ObjectServer.Utility;

namespace ObjectServer
{
    [Serializable]
    [JsonObject("objectserver-config")]
    [XmlRoot("objectserver-config")]
    public sealed class Config
    {
        public Config()
        {
            this.LogPath = null;
            this.LogLevel = "info";
            this.SessionTimeout = new TimeSpan(0, 20, 0);
        }

        private string rootPassword;

        /// <summary>
        /// 配置文件路径
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        public string ConfigurationPath { get; set; }

        [XmlElement("db-type")]
        [JsonProperty("db-type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public Backend.DatabaseType DbType { get; set; }

        [XmlElement("db-host")]
        [JsonProperty("db-host")]
        public string DBHost { get; set; }

        [XmlElement("db-port")]
        [JsonProperty("db-port")]
        public int DBPort { get; set; }

        [XmlElement("db-user")]
        [JsonProperty("db-user")]
        public string DBUser { get; set; }

        [XmlElement("db-password")]
        [JsonProperty("db-password")]
        public string DBPassword { get; set; }

        /// <summary>
        /// 指定连接的数据库名，如果没有指定，则可以连接到用户所属的多个数据库。
        /// </summary>
        [XmlElement("db-name")]
        [JsonProperty("db-name")]
        public string DbName { get; set; }

        [XmlElement("module-path")]
        [JsonProperty("module-path", Required = Required.Always)]
        public string ModulePath { get; set; }

        [XmlElement("debug")]
        [JsonProperty("debug")]
        public bool Debug { get; set; }

        [XmlElement("root-password")]
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

        [XmlElement("log-path")]
        [JsonProperty("log-path")]
        public string LogPath { get; set; }

        [XmlElement("log-level")]
        [JsonProperty("log-level")]
        public string LogLevel { get; set; }

        [XmlElement("session-timeout")]
        [JsonProperty("session-timeout")]
        public TimeSpan SessionTimeout { get; set; }

        [JsonIgnore]
        [XmlIgnore]
        public string RootPasswordHash { get; private set; }

    }
}
