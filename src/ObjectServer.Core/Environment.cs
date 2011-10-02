using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;

using ObjectServer.Exceptions;
using Newtonsoft.Json;

namespace ObjectServer
{
    /// <summary>
    /// SINGLETON
    /// 框架的全局入口点
    /// </summary>
    public sealed class Environment : IDisposable
    {
        private static readonly Environment s_instance = new Environment();

        private bool disposed = false;
        private Config config;
        private bool initialized;
        private DBProfileCollection databaseProfiles = new DBProfileCollection();
        private ModuleCollection modules = new ModuleCollection();
        private SessionStore sessionStore = new SessionStore();
        private IExportedService exportedService = ServiceDispatcher.CreateDispatcher();

        private Environment()
        {
        }

        ~Environment()
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
                    //处置托管资源
                }

                //处置非托管资源
                this.databaseProfiles.Dispose();

                disposed = true;
                LoggerProvider.EnvironmentLogger.Info("The platform environment has been closed.");
            }
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
            s_instance.InitializeInternal(cfg);
        }

        //这个做实际的初始化工作
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initialize(Config cfg)
        {
            s_instance.InitializeInternal(cfg);
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

            var cfg = new Config();
            s_instance.InitializeInternal(cfg);
        }


        private void InitializeInternal(Config cfg)
        {
            TryInitialize(cfg);
        }

        private static void TryInitialize(Config cfg)
        {
            if (cfg == null)
            {
                throw new ArgumentNullException("cfg");
            }

            //日志子系统必须最先初始化
            ConfigurateLogger(cfg);
            LoggerProvider.EnvironmentLogger.Info("The Logging Subsystem has been initialized");

            LoggerProvider.EnvironmentLogger.Info(() => "Initializing the Session Storage Subsystem...");
            string sessionProviderType = cfg.SessionProvider;
            if (string.IsNullOrEmpty(sessionProviderType))
            {
                sessionProviderType = typeof(ObjectServer.StaticSessionStoreProvider).AssemblyQualifiedName;
            }
            s_instance.sessionStore.Initialize(sessionProviderType);

            //查找所有模块并加载模块元信息
            LoggerProvider.EnvironmentLogger.Info(() => "Initializing the Module Management Subsystem...");
            if (!string.IsNullOrEmpty(cfg.ModulePath))
            {
                s_instance.modules.Initialize(cfg);
            }

            s_instance.config = cfg;
            s_instance.initialized = true;
            LoggerProvider.EnvironmentLogger.Info(() => "The environment has been initialized.");

            LoggerProvider.EnvironmentLogger.Info(() => "Initializing databases...");
            s_instance.databaseProfiles.Initialize(cfg);

            LoggerProvider.EnvironmentLogger.Info(() => "The ObjectServer Platform is ready to load the Core Module...");
        }

        public static void Shutdown()
        {
            LoggerProvider.EnvironmentLogger.Info("The whole system will be halt...");
            if (s_instance.initialized)
            {
                s_instance.Dispose(true);
            }
        }


        public static bool Initialized
        {
            get
            {
                return s_instance.initialized;
            }
        }


        internal static DBProfileCollection DBProfiles
        {
            get
            {
                return Instance.databaseProfiles;
            }
        }

        internal static ModuleCollection Modules
        {
            get
            {
                return Instance.modules;
            }
        }

        public static SessionStore SessionStore
        {
            get
            {
                return Instance.sessionStore;
            }
        }

        private static void ConfigurateLogger(Config cfg)
        {
            LoggerProvider.Configurate(cfg);
        }


        public static Config Configuration
        {
            get
            {
                return Instance.config;
            }
        }

        public static IExportedService ExportedService
        {
            get
            {
                return Instance.exportedService;

            }
        }

        private static Environment Instance
        {
            get
            {
                if (!s_instance.initialized)
                {
                    throw new Exception(
                        "尚未初始化系统，请调用 ObjectServerStarter.Initialize() 初始化");
                }

                return s_instance;
            }
        }
    }
}
