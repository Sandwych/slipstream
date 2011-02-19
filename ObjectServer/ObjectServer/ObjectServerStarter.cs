using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
        private Pooler pooler = new Pooler();

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
            s_instance.config = cfg;

            if (!string.IsNullOrEmpty(cfg.ModulePath))
            {
                Module.Module.LookupAllModules();
            }

            s_instance.initialized = true;
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

        public static Pooler Pooler
        {
            get
            {
                return s_instance.pooler;
            }
        }


    }
}
