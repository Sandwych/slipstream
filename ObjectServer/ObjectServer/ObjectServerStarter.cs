using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using log4net;

using Newtonsoft.Json;

namespace ObjectServer
{
    /// <summary>
    /// SINGLETON
    /// TODO 线程安全
    /// </summary>
    public sealed class ObjectServerStarter
    {
        private static readonly ObjectServerStarter s_instance = new ObjectServerStarter();

        private Configuration config;
        private bool initialized;
        private Database pooler = new Database();

        private ObjectServerStarter()
        {
            this.initialized = false;
        }

        public static void Initialize(string configPath)
        {
            var json = File.ReadAllText(configPath, Encoding.UTF8);
            var cfg = JsonConvert.DeserializeObject<Configuration>(json);
            Initialize(cfg);
        }

        //这个做实际的初始化工作
        public static void Initialize(Configuration cfg)
        {
            if (cfg == null)
            {
                throw new ArgumentNullException("cfg");
            }

            if (!string.IsNullOrEmpty(cfg.ModulePath))
            {
                Module.LookupAllModules(cfg.ModulePath);
            }


            lock (typeof(ObjectServerStarter))
            {
                ConfigurateLog4net(cfg);

                s_instance.config = cfg;
                s_instance.initialized = true;

            }
        }

        private static void ConfigurateLog4net(Configuration cfg)
        {
            log4net.Appender.IAppender appender;
            var layout = new log4net.Layout.SimpleLayout();

            if (string.IsNullOrEmpty(cfg.LogPath))
            {
                appender = new log4net.Appender.ConsoleAppender
                {
                    Layout = layout,
                };

                if (!cfg.Debug)
                {
                    //TODO: 添加非调试的级别限制
                    //var filter = new log4net.Filter.LevelRangeFilter 
                    //{ LevelMax = log4net.Core.Level.Debug,  };
                }
            }
            else
            {
                if (System.IO.Directory.Exists(cfg.LogPath))
                {
                    throw new DirectoryNotFoundException(cfg.LogPath);
                }

                appender = new log4net.Appender.RollingFileAppender()
                {
                    Layout = layout,
                    Encoding = Encoding.UTF8,
                };
            }

            log4net.Config.BasicConfigurator.Configure(appender);
        }

        /// <summary>
        /// 为调试及测试而初始化
        /// </summary>
        public static void Initialize()
        {
            if (s_instance.initialized)
            {
                return;
            }

            var cfg = new Configuration()
            {
                ConfigurationPath = null,
                DbType = ObjectServer.Backend.DatabaseType.Postgresql,
                DbHost = "localhost",
                DbName = "objectserver",
                DbPassword = "objectserver",
                DbPort = 5432,
                DbUser = "objectserver",
                ModulePath = "modules",
                RootPassword = "root",
                Debug = true,
                LogLevel = "debug",
            };
            Initialize(cfg);
        }

        public static Configuration Configuration
        {
            get
            {
                if (s_instance.config == null)
                {
                    throw new ArgumentException(
                        "尚未初始化系统，请调用 ObjectServerStarter.Initialize() 初始化");
                }
                return s_instance.config;
            }
        }

        public static bool Initialized
        {
            get { return s_instance.initialized; }
        }

        internal static Database Pooler
        {
            get
            {
                return s_instance.pooler;
            }
        }


    }
}
