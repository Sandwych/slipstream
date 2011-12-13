using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;

using Autofac;
using Newtonsoft.Json;

using ObjectServer.Exceptions;

namespace ObjectServer
{
    /// <summary>
    /// SINGLETON
    /// 平台的全局入口点
    /// </summary>
    public sealed class SlipstreamEnvironment : IDisposable
    {
        private static SlipstreamEnvironment s_instance;

        private bool _disposed = false;
        private readonly Autofac.IContainer _rootContainer;
        private readonly IDbDomainManager _dbDomains;
        private readonly IModuleManager _modules;
        private Config _config;
        private bool _initialized;
        private readonly ISlipstreamService _exportedService;

        private SlipstreamEnvironment(Config cfg)
        {
            var containerBuilder = new Autofac.ContainerBuilder();
            RegisterInsideComponents(containerBuilder, cfg);

            this._rootContainer = containerBuilder.Build();

            this._dbDomains = this._rootContainer.Resolve<IDbDomainManager>();
            this._modules = this._rootContainer.Resolve<IModuleManager>();
            this._exportedService = this._rootContainer.Resolve<ISlipstreamService>();
        }

        private static void RegisterInsideComponents(ContainerBuilder containerBuilder, Config cfg)
        {
            containerBuilder.RegisterType<DbDomainManager>()
                .As<IDbDomainManager>().SingleInstance();
            containerBuilder.RegisterType<ModuleManager>()
                .As<IModuleManager>().SingleInstance();
            containerBuilder.RegisterType<SlipstreamService>()
                .As<ISlipstreamService>().SingleInstance();
            containerBuilder.RegisterInstance<Data.IDataProvider>(Data.DataProvider.CreateDataProvider(cfg.DbType))
                .SingleInstance();

            //加载内置的 DataProviders
            /*
            var assemblies = new Assembly[] {
                Assembly.GetExecutingAssembly() 
            };
            containerBuilder.RegisterAssemblyTypes(assemblies)
                .Where(t => t.Name.EndsWith("DataProvider"))
                .AsImplementedInterfaces();
            */
        }

        ~SlipstreamEnvironment()
        {
            Dispose(false);
        }

        #region IDisposable Members

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    //处置托管资源
                }

                //处置非托管资源
                this._rootContainer.Dispose();

                _disposed = true;
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
            TryInitialize(cfg);
        }

        /// <summary>
        /// Do the real initialization job
        /// </summary>
        /// <param name="config"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initialize(Config config)
        {
            TryInitialize(config);
        }


        /// <summary>
        /// 为调试及测试而初始化
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initialize()
        {
            if (s_instance != null && s_instance._initialized)
            {
                return;
            }
            else
            {
                var cfg = new Config();
                TryInitialize(cfg);
            }
        }

        private static void TryInitialize(Config cfg)
        {
            if (cfg == null)
            {
                throw new ArgumentNullException("cfg");
            }

            //We must initialize the logging subsystem first
            ConfigurateLogger(cfg);
            LoggerProvider.EnvironmentLogger.Info("The Logging Subsystem has been _initialized");

            var env = new SlipstreamEnvironment(cfg);
            s_instance = env;



            //查找所有模块并加载模块元信息
            LoggerProvider.EnvironmentLogger.Info(() => "Module Management Subsystem Initializing...");
            if (!string.IsNullOrEmpty(cfg.ModulePath))
            {
                s_instance._modules.Initialize(cfg);
            }

            s_instance._config = cfg;
            s_instance._initialized = true;
            LoggerProvider.EnvironmentLogger.Info(() => "The environment has been _initialized.");

            LoggerProvider.EnvironmentLogger.Info(() => "Initializing databases...");
            s_instance._dbDomains.Initialize(cfg);

            LoggerProvider.EnvironmentLogger.Info(() => "Runtime environment successfully _initialized.");
        }

        public static void Shutdown()
        {
            LoggerProvider.EnvironmentLogger.Info("The whole system will be halt...");
            if (s_instance._initialized)
            {
                s_instance.Dispose(true);
            }
        }


        public static bool Initialized
        {
            get
            {
                return s_instance != null && s_instance._initialized;
            }
        }


        internal static IDbDomainManager DbDomains
        {
            get
            {
                return Instance._dbDomains;
            }
        }

        internal static IModuleManager Modules
        {
            get
            {
                return Instance._modules;
            }
        }

        /*
        public static SessionStore SessionStore
        {
            get
            {
                return Instance.sessionStore;
            }
        }
        */

        private static void ConfigurateLogger(Config cfg)
        {
            LoggerProvider.Configurate(cfg);
        }


        public static Config Configuration
        {
            get
            {
                return Instance._config;
            }
        }

        public static ISlipstreamService RootService
        {
            get
            {
                return Instance._exportedService;

            }
        }

        public static Autofac.IContainer RootContainer
        {
            get { return Instance._rootContainer; }
        }

        private static SlipstreamEnvironment Instance
        {
            get
            {
                if (!s_instance._initialized)
                {
                    throw new Exception(
                        "尚未初始化系统，请调用 ObjectServerStarter.Setup() 初始化");
                }

                return s_instance;
            }
        }
    }
}
