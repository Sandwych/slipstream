using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;

namespace ObjectServer
{
    /// <summary>
    /// 配置类
    /// <remarks>多个线程将会读取此类实例的字段</remarks>
    /// </summary>
    [Serializable]
    [XmlRoot("objectserver-config")]
    public sealed class Config
    {
        public Config()
        {
            this.Role = ServerRoles.Standalone;
            this.DbType = "postgres";
            this.DBHost = "localhost";
            this.DBPort = 5432;
            this.DBUser = "objectserver";
            this.DBPassword = "objectserver";

            this.SessionTimeoutMinutes = 20;

            var defaultLogPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ObjectServer", "log");
            this.LogPath = defaultLogPath;
            this.LoggingSql = false;

#if DEBUG
            this.Debug = true;
            this.LogLevel = "debug";
            this.LogToConsole = true;
#else
            this.Debug = false;
            this.LogLevel = "info";
            this.LoggingSql = false;
            this.LogToConsole = false;
#endif

#if DEBUG
            this.RpcHandlerMax = Environment.ProcessorCount;
#else
            this.RpcHandlerMax = Environment.ProcessorCount * 4;
#endif
            this.RpcHandlerUrl = "inproc://rpc-handlers";
            this.RpcHostUrl = "inproc://rpc-entrance";
            this.CommanderUrl = "inproc://controller";
            this.HttpListenUrl = "http://localhost:9287/";

            this.ModulePath = "Modules";
            this.RootPassword = "root";
        }

        /// <summary>
        /// 配置文件路径
        /// </summary>
        [XmlIgnore]
        public string ConfigurationPath { get; set; }

        [XmlElement("role")]
        public ServerRoles Role { get; set; }

        [XmlElement("db-type", IsNullable = false)]
        public string DbType { get; set; }

        [XmlElement("db-host")]
        public string DBHost { get; set; }

        [XmlElement("db-port")]
        public int DBPort { get; set; }

        [XmlElement("db-user")]
        public string DBUser { get; set; }

        [XmlElement("db-password")]
        public string DBPassword { get; set; }

        /// <summary>
        /// 指定连接的数据库名，如果没有指定，则可以连接到用户所属的多个数据库。
        /// </summary>
        [XmlElement("db-name")]
        public string DbName { get; set; }

        [XmlElement("module-path")]
        public string ModulePath { get; set; }

        [XmlElement("debug")]
        public bool Debug { get; set; }

        [XmlElement("root-password", IsNullable = false)]
        public string RootPassword { get; set; }

        [XmlElement("log-to-console")]
        public bool LogToConsole { get; set; }

        [XmlElement("log-path")]
        public string LogPath { get; set; }

        [XmlElement("log-level")]
        public string LogLevel { get; set; }

        [XmlElement("logging-sql")]
        public bool LoggingSql { get; set; }

        [XmlElement("session-timeout-minutes")]
        public int SessionTimeoutMinutes { get; set; }

        [XmlElement("session-provider", IsNullable = false)]
        public string SessionProvider { get; set; }

        [XmlElement("rpc-handler-max")]
        public int RpcHandlerMax { get; set; }

        [XmlElement("rpc-handler-url")]
        public string RpcHandlerUrl { get; set; }

        [XmlElement("rpc-host-url")]
        public string RpcHostUrl { get; set; }

        [XmlElement("controller-url")]
        public string CommanderUrl { get; set; }

        [XmlElement("http-listen-url")]
        public string HttpListenUrl { get; set; }
    }
}
