using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;

using Autofac;
using Autofac.Integration.Mef;
using Newtonsoft.Json;

using SlipStream.Exceptions;

namespace SlipStream {
    /// <summary>
    /// SINGLETON
    /// 平台的全局入口点
    /// </summary>
    public sealed class SlipstreamEnvironment : IDisposable {
        private static SlipstreamEnvironment s_instance;

        private bool _disposed = false;
        private readonly Autofac.IContainer _rootContainer;
        private readonly IDbDomainManager _dbDomains;
        private readonly IModuleManager _modules;
        private ShellSettings _shellSettings;
        private bool _initialized;
        private readonly ISlipstreamService _exportedService;

        private SlipstreamEnvironment(ShellSettings cfg) {
            var containerBuilder = new Autofac.ContainerBuilder();
            RegisterInsideComponents(containerBuilder, cfg);

            this._rootContainer = containerBuilder.Build();

            this._shellSettings = this._rootContainer.Resolve<ShellSettings>();
            this._dbDomains = this._rootContainer.Resolve<IDbDomainManager>();
            this._modules = this._rootContainer.Resolve<IModuleManager>();
            this._exportedService = this._rootContainer.Resolve<ISlipstreamService>();
        }

        private static void RegisterInsideComponents(ContainerBuilder containerBuilder, ShellSettings cfg) {
            containerBuilder.RegisterInstance(cfg).SingleInstance();
            containerBuilder.RegisterType<DbDomainManager>()
                .As<IDbDomainManager>().SingleInstance();
            containerBuilder.RegisterType<ModuleManager>()
                .As<IModuleManager>().SingleInstance();
            containerBuilder.RegisterType<SlipstreamService>()
                .As<ISlipstreamService>().SingleInstance();

            Data.IDataProvider dataProvider = null;
            switch (cfg.DbType) {
                case "mssql":
                    dataProvider = new Data.Mssql.MssqlDataProvider();
                    break;

                case "postgres":
                    dataProvider = new Data.Postgresql.PgDataProvider();
                    break;

                default:
                    throw new NotSupportedException("Not supported database: " + cfg.DbType);
            }

            containerBuilder.RegisterInstance<Data.IDataProvider>(dataProvider)
                .SingleInstance();

            //加注册 MEF 管理的插件

            //TODO 目前都是放在一个 dll 里，以后要分开
            //var selfCatalog = new AssemblyCatalog(System.Reflection.Assembly.GetExecutingAssembly());
            //containerBuilder.RegisterComposablePartCatalog(selfCatalog);           
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

        ~SlipstreamEnvironment() {
            Dispose(false);
        }

        #region IDisposable Members

        private void Dispose(bool disposing) {
            if (!this._disposed) {
                if (disposing) {
                    //处置托管资源
                }

                //处置非托管资源
                this._rootContainer.Dispose();

                _disposed = true;
                LoggerProvider.EnvironmentLogger.Info("The platform environment has been closed.");
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion


        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initialize(string configPath) {
            var json = File.ReadAllText(configPath, Encoding.UTF8);
            var cfg = JsonConvert.DeserializeObject<ShellSettings>(json);
            TryInitialize(cfg);
        }

        /// <summary>
        /// Do the real initialization job
        /// </summary>
        /// <param name="config"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initialize(ShellSettings config) {
            TryInitialize(config);
        }


        /// <summary>
        /// 为调试及测试而初始化
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Initialize() {
            if (s_instance != null && s_instance._initialized) {
                return;
            }
            else {
                var cfg = new ShellSettings();
                TryInitialize(cfg);
            }
        }

        private static void TryInitialize(ShellSettings shellSettings) {
            if (shellSettings == null) {
                throw new ArgumentNullException("shellSettings");
            }

            //We must initialize the logging subsystem first
            ConfigurateLogger(shellSettings);
            LoggerProvider.EnvironmentLogger.Info("The Logging Subsystem has been _initialized");

            var env = new SlipstreamEnvironment(shellSettings);
            s_instance = env;

            s_instance._shellSettings = shellSettings;
            s_instance._initialized = true;
            LoggerProvider.EnvironmentLogger.Info(() => "Runtime environment successfully initialized.");
        }

        public static void Shutdown() {
            LoggerProvider.EnvironmentLogger.Info("The whole system will be halt...");
            if (s_instance._initialized) {
                s_instance.Dispose(true);
            }
        }


        public static bool Initialized {
            get {
                return s_instance != null && s_instance._initialized;
            }
        }


        internal static IDbDomainManager DbDomains {
            get {
                return Instance._dbDomains;
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

        private static void ConfigurateLogger(ShellSettings cfg) {
            LoggerProvider.Configurate(cfg);
        }


        public static ShellSettings Settings {
            get {
                return Instance._shellSettings;
            }
        }

        public static ISlipstreamService RootService {
            get {
                return Instance._exportedService;

            }
        }

        public static Autofac.IContainer RootContainer {
            get { return Instance._rootContainer; }
        }

        private static SlipstreamEnvironment Instance {
            get {
                if (s_instance == null || !s_instance._initialized) {
                    throw new Exception(
                        "尚未初始化系统，请调用 SlipstreamEnvironment.Initialize() 初始化");
                }

                return s_instance;
            }
        }
    }
}
