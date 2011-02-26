using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using log4net;

using Newtonsoft.Json;

namespace ObjectServer
{
    /// <summary>
    /// SINGLETON
    /// 框架的全局入口点
    /// </summary>
    public sealed class ObjectServerStarter : IDisposable
    {
        private static readonly ObjectServerStarter s_instance = new ObjectServerStarter();

        private bool disposed = false;
        private Config config;
        private bool initialized;
        private DatabasePool dbPool = new DatabasePool();
        private ModulePool modulePool = new ModulePool();
        private SessionStore sessionStore = new SessionStore();

        private ObjectServerStarter()
        {
        }

        ~ObjectServerStarter()
        {
            Dispose(false);
        }

        #region IDisposable 成员

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.dbPool.Dispose();
                }

            }
            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initialize(string configPath)
        {
            var json = File.ReadAllText(configPath, Encoding.UTF8);
            var cfg = JsonConvert.DeserializeObject<Config>(json);
            Initialize(cfg);
        }

        //这个做实际的初始化工作
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initialize(Config cfg)
        {
            if (cfg == null)
            {
                throw new ArgumentNullException("cfg");
            }

            s_instance.sessionStore.Initialize(cfg);
            s_instance.dbPool.Initialize(cfg);

            //查找所有模块并加载模块元信息
            if (!string.IsNullOrEmpty(cfg.ModulePath))
            {
                s_instance.modulePool.Initialize(cfg);
            }

            ConfigurateLog4net(cfg);

            s_instance.config = cfg;
            s_instance.initialized = true;
        }


        /// <summary>
        /// 为调试及测试而初始化
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initialize()
        {
            if (s_instance.initialized)
            {
                return;
            }

            var cfg = new Config()
            {
                ConfigurationPath = null,
                DbType = ObjectServer.Backend.DatabaseType.Postgresql,
                DBHost = "localhost",
                DbName = "objectserver",
                DBPassword = "objectserver",
                DBPort = 5432,
                DBUser = "objectserver",
                ModulePath = "modules",
                RootPassword = "root",
                Debug = true,
                LogLevel = "debug",
                SessionTimeout = TimeSpan.MaxValue,
            };
            Initialize(cfg);
        }


        public static bool Initialized
        {
            get { return s_instance.initialized; }
        }


        internal static DatabasePool DatabasePool
        {
            get
            {
                return s_instance.dbPool;
            }
        }

        internal static ModulePool ModulePool
        {
            get { return s_instance.modulePool; }
        }

        public static SessionStore SessionStore
        {
            get { return s_instance.sessionStore; }
        }

        private static void ConfigurateLog4net(Config cfg)
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


        public static Config Configuration
        {
            get
            {
                if (s_instance.config == null)
                {
                    throw new Exception(
                        "尚未初始化系统，请调用 ObjectServerStarter.Initialize() 初始化");
                }
                return s_instance.config;
            }
        }


    }
}
