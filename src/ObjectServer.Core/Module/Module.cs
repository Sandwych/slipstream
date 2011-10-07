using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

using NHibernate.SqlCommand;

using ObjectServer.Runtime;
using ObjectServer.Data;
using ObjectServer.Core;
using ObjectServer.Utility;

namespace ObjectServer
{
    /// <summary>
    /// TODO: 线程安全
    /// </summary>
    [Serializable]
    [XmlRoot("module-metadata")]
    public sealed class Module
    {
        private readonly List<Assembly> allAssembly = new List<Assembly>();
        private static readonly Module s_coreModule;
        private readonly List<IResource> resources = new List<IResource>();

        private static readonly string[] s_coreInitDataFiles = new string[] 
        {
            //下面的初始化顺序很重要，底层的放在前，高层的放在后
            "ObjectServer.Core.Data.InitData.xml",
            "ObjectServer.Core.Data.Security.xml",            
            "ObjectServer.Core.Data.Views.xml",
            "ObjectServer.Core.Data.Menus.xml",
        };

        static Module()
        {
            var asm = Assembly.GetExecutingAssembly();
            s_coreModule = new Module()
            {
                Name = StaticSettings.CoreModuleName,
                Depends = new string[] { },
                AutoLoad = true,
                Version = asm.GetName().Version,
            };

            var asmAttrs = asm.GetCustomAttributes(false);
            foreach (var attr in asmAttrs)
            {
                if (attr is AssemblyCompanyAttribute)
                {
                    var aca = (AssemblyCompanyAttribute)attr;
                    s_coreModule.Author = aca.Company;
                }
            }
        }

        public Module()
        {
            //设置属性默认值
            this.Version = Version.Parse("0.0.0.0");
            this.Depends = new string[] { };
            this.Dlls = new string[] { };
            this.IsDemo = false;
        }

        #region Serializable Fields
        [XmlElement("name", IsNullable = false)]
        public string Name { get; set; }

        [XmlElement("label")]
        public string Label { get; set; }

        [XmlElement("version")]
        public Version Version { get; set; }

        [XmlElement("info")]
        public string Info { get; set; }

        [XmlElement("demo")]
        public bool IsDemo { get; set; }

        [XmlElement("author")]
        public string Author { get; set; }

        [XmlElement("url")]
        public string Url { get; set; }

        [XmlElement("license")]
        public string License { get; set; }

        [XmlArray("sources")]
        [XmlArrayItem("file")]
        public string[] SourceFiles { get; set; }

        /// <summary>
        /// 模块初安装的时候导入的文件
        /// </summary>
        [XmlArray("init-files", IsNullable = true)]
        [XmlArrayItem("file", IsNullable = false)]
        public string[] InitFiles { get; set; }

        /// <summary>
        /// 模块升级时导入的文件
        /// </summary>
        [XmlArray("upgrade-files", IsNullable = true)]
        [XmlArrayItem("file", IsNullable = false)]
        public string[] UpgradeFiles { get; set; }

        [XmlElement("source-language")]
        public string SourceLanguage { get; set; }

        [XmlArray("scripts")]
        [XmlArrayItem("file")]
        public string[] Scripts { get; set; }

        [XmlElement("auto-load")]
        public bool AutoLoad { get; set; }

        [XmlArray("depends")]
        [XmlArrayItem("depend")]
        public string[] Depends { get; set; }

        [XmlArray("dlls")]
        [XmlArrayItem("file")]
        public string[] Dlls { get; set; }

        #endregion

        public void Load(ITransactionContext ctx, ModuleUpdateAction action)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("scope");
            }

            LoggerProvider.EnvironmentLogger.Info(() => string.Format("Loading module: [{0}]", this.Name));

            this.resources.Clear();

            if (this.Name == "core")
            {
#if DEBUG //调试模式不捕获异常，以便于调试
                this.LoadCoreModule(ctx, action);
#else
                try
                {
                    this.LoadCoreModule(ctx, action);
                }
                catch (Exception ex)
                {
                    var msg = "Failed to load core module";
                    LoggerProvider.EnvironmentLogger.Fatal(msg, ex);
                    throw new Exceptions.InitializationException(msg, ex);
                }
#endif
            }
            else
            {
                this.LoadAdditionalModule(ctx, action);
            }
        }

        private void LoadAdditionalModule(ITransactionContext scope, ModuleUpdateAction action)
        {
            Debug.Assert(scope != null);

            LoggerProvider.EnvironmentLogger.Info(() => "Loading precompiled assemblies...");

            if (this.Dlls != null)
            {
                this.LoadStaticAssemblies();
            }

            if (!string.IsNullOrEmpty(this.Path))
            {
                this.LoadDynamicAssembly();
            }

            this.InitializeAllResources(scope, action);

            this.LoadModuleData(scope, action);

            LoggerProvider.EnvironmentLogger.Info(() => string.Format("Module [{0}] has been loaded.", this.Name));
        }

        private void LoadCoreModule(ITransactionContext tc, ModuleUpdateAction action)
        {
            Debug.Assert(tc != null);

            var a = typeof(ObjectServer.Core.ModuleModel).Assembly;
            this.RegisterResourcesWithinAssembly(a);

            this.InitializeAllResources(tc, action);

            //加载核心模块数据
            if (action == ModuleUpdateAction.ToInstall)
            {
                LoggerProvider.EnvironmentLogger.Info(() => "Importing data for the Core Module...");
                var importer = new Model.XmlDataImporter(tc, this.Name);
                foreach (var resPath in s_coreInitDataFiles)
                {
                    using (var resStream = a.GetManifestResourceStream(resPath))
                    {
                        LoggerProvider.EnvironmentLogger.Info(() => "Importing data file: [" + resPath + "]");
                        importer.Import(resStream);
                        resStream.Close();
                    }
                }
            }
        }

        private void InitializeAllResources(ITransactionContext tc, ModuleUpdateAction action)
        {
            //注册并初始化所有资源
            foreach (var r in this.resources)
            {
                tc.Resources.RegisterResource(r);
            }

            tc.Resources.InitializeAllResources(tc, action != ModuleUpdateAction.NoAction);
        }

        private void LoadModuleData(ITransactionContext scope, ModuleUpdateAction action)
        {
            Debug.Assert(scope != null);

            LoggerProvider.EnvironmentLogger.Info(() => "Importing data...");

            if (action == ModuleUpdateAction.ToInstall && this.InitFiles != null && this.InitFiles.Length > 0)
            {
                LoggerProvider.EnvironmentLogger.Info(() => "Importing data for installation...");
                var files = this.InitFiles;
                this.ImportDataFiles(scope, files);
            }

            if (action == ModuleUpdateAction.ToUpgrade && this.UpgradeFiles != null && this.UpgradeFiles.Length > 0)
            {
                LoggerProvider.EnvironmentLogger.Info(() => "Importing data for upgrade...");
                var files = this.UpgradeFiles;
                this.ImportDataFiles(scope, files);
            }
        }

        private void ImportDataFiles(ITransactionContext scope, string[] files)
        {
            var importer = new Model.XmlDataImporter(scope, this.Name);
            foreach (var dataFile in files)
            {
                var dataFilePath = System.IO.Path.Combine(this.Path, dataFile);
                LoggerProvider.EnvironmentLogger.Info(() => "Importing data file: [" + dataFilePath + "]");
                importer.Import(dataFilePath);
            }
        }

        private void LoadDynamicAssembly()
        {
            var a = CompileSourceFiles(this.Path);
            this.AllAssemblies.Add(a);
            this.RegisterResourcesWithinAssembly(a);
        }

        private void RegisterResourcesWithinAssembly(Assembly assembly)
        {
            Debug.Assert(assembly != null);

            LoggerProvider.EnvironmentLogger.Info(() => string.Format(
                "Registering all resources in DLL[{0}], Path=[{1}]...", assembly.FullName, assembly.Location));

            var types = GetStaticModelsFromAssembly(assembly);

            foreach (var t in types)
            {
                var res = AbstractResource.CreateStaticResourceInstance(t);
                res.Module = this.Name;
                this.resources.Add(res);
            }
        }

        private static Type[] GetStaticModelsFromAssembly(Assembly assembly)
        {
            Debug.Assert(assembly != null);

            var types = assembly.GetTypes();
            var result = new List<Type>();
            foreach (var t in types)
            {
                var attr = Attribute.GetCustomAttribute(t, typeof(ResourceAttribute), false);
                if (attr != null)
                {
                    result.Add(t);
                }
            }
            return result.ToArray();
        }

        private void LoadStaticAssemblies()
        {
            Debug.Assert(this.Dlls != null);

            foreach (var dll in this.Dlls)
            {
                var a = Assembly.LoadFile(dll);
                this.allAssembly.Add(a);
                this.RegisterResourcesWithinAssembly(a);
            }
        }

        [XmlIgnore]
        public ICollection<Assembly> AllAssemblies { get { return this.allAssembly; } }

        [XmlIgnore]
        public string Path { get; set; }

        public static Module Deserialize(string moduleFilePath)
        {
            if (string.IsNullOrEmpty(moduleFilePath))
            {
                throw new ArgumentNullException("moduleFilePath");
            }

            var xs = new XmlSerializer(typeof(Module));

            using (var fs = File.OpenRead(moduleFilePath))
            {
                var module = (Module)xs.Deserialize(fs);
                fs.Close();
                return module;
            }
        }

        public static Module Deserialize(Stream input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var xs = new XmlSerializer(typeof(Module));

            var module = (Module)xs.Deserialize(input);
            return module;
        }

        public void AddToDatabase(IDbContext dbctx)
        {
            if (dbctx == null)
            {
                throw new ArgumentNullException("dbctx");
            }

            var state = ModuleModel.States.Uninstalled;
            if (this.AutoLoad)
            {
                state = ModuleModel.States.ToInstall;
            }

            var insertSql = SqlString.Parse(
                "insert into core_module(name, state, label, version, demo, author, info) values(?,?,?,?,?,?,?)");
            dbctx.Execute(insertSql,
                this.Name, state, this.Label, this.Version.ToString(), this.IsDemo, this.Author, this.Info);
        }

        public static Module CoreModule { get { return s_coreModule; } }

        private Assembly CompileSourceFiles(string moduleDir)
        {
            Debug.Assert(!string.IsNullOrEmpty(moduleDir));

            LoggerProvider.EnvironmentLogger.Info(String.Format(
                "Compiling source files of the module [{0}]...", this.Name));

            var sourceFiles = new List<string>();
            foreach (var file in this.SourceFiles)
            {
                sourceFiles.Add(System.IO.Path.Combine(moduleDir, file));
            }

            //编译模块程序并注册所有对象
            var compiler = CompilerProvider.GetCompiler(this.SourceLanguage);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var a = compiler.CompileFromFile(sourceFiles);
            stopwatch.Stop();
            var time = stopwatch.Elapsed;
            LoggerProvider.EnvironmentLogger.Info(String.Format("Elapsed time: [{0}]", time));
            LoggerProvider.EnvironmentLogger.Info(String.Format(
                "The module [{0}] has been compiled successfully.", this.Name));
            return a;
        }

        public IEnumerable<IResource> Resources
        {
            get
            {
                return this.resources;
            }
        }

        private static void ResourceDependencySort(IList<IResource> resList)
        {
            Debug.Assert(resList != null);

            var objDepends = new Dictionary<string, string[]>(resList.Count);
            foreach (var res in resList)
            {
                objDepends.Add(res.Name, res.GetReferencedObjects());
            }

            resList.DependencySort(m => m.Name, m => objDepends[m.Name]);
        }


    }
}
