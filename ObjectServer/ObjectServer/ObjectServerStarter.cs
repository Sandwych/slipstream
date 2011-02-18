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

        private ObjectServerStarter()
        {
        }

        public static void Initialize(string configPath)
        {
            var json = File.ReadAllText(configPath, Encoding.UTF8);
            var cfg = JsonConvert.DeserializeObject<Configuration>(json);
            Initialize(cfg);
        }

        public static void Initialize(Configuration cfg)
        {
            if (cfg == null)
            {
                throw new ArgumentNullException("cfg");
            }
            s_instance.config = cfg;
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


    }
}
