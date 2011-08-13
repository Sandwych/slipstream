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

        private static readonly string[] s_coreDataFiles = new string[] 
        {
            //TODO 下面的初始化顺序很重要，底层的放在前，高层的放在后
            "ObjectServer.Core.Data.InitData.xml",
            "ObjectServer.Core.Data.Security.xml",            
            "ObjectServer.Core.Data.Views.xml",
            "ObjectServer.Core.Data.Menus.xml",
        };

        static Module()
        {
            s_coreModule = new Module()
            {
                Name = StaticSettings.CoreModuleName,
                State = ModuleStatus.Activated,
                Depends = new string[] { },
                AutoLoad = true,
            };
        }

        public Module()
        {
            //设置属性默认值
            this.Depends = new string[] { };
            this.Dlls = new string[] { };
            this.State = ModuleStatus.Deactivated;
        }

        #region Serializable Fields
        [XmlElement("name", IsNullable = false)]
        public string Name { get; set; }

        [XmlElement("label")]
        public string Label { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlArray("sources")]
        [XmlArrayItem("file")]
        public string[] SourceFiles { get; set; }

        [XmlArray("data-files")]
        [XmlArrayItem("file")]
        public string[] DataFiles { get; set; }

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

        public void Load(IServiceScope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            LoggerProvider.PlatformLogger.Info(() => string.Format("Loading module: [{0}]", this.Name));

            this.resources.Clear();

            if (this.Name == "core")
            {
#if DEBUG //调试模式不捕获异常，以便于调试
                this.LoadCoreModule(scope);
#else
                try
                {
                    this.LoadCoreModule(ctx);
                }
                catch (Exception ex)
                {
                    var msg = "Failed to load core module";
                    Logger.Fatal(msg, ex);
                    throw new InitializationException(msg, ex);
                }
#endif
            }
            else
            {
                this.LoadAdditionalModule(scope);
            }
        }

        private void LoadAdditionalModule(IServiceScope scope)
        {
            Debug.Assert(scope != null);

            LoggerProvider.PlatformLogger.Info(() => "Loading precompiled assemblies...");
            var dbProfile = Platform.DBProfiles.TryGetDBProfile(scope.Session);

            if (this.Dlls != null)
            {
                this.LoadStaticAssemblies(dbProfile);
            }

            if (!string.IsNullOrEmpty(this.Path))
            {
                this.LoadDynamicAssembly(dbProfile, dbProfile);
            }

            dbProfile.InitializeAllResources();

            if (this.DataFiles != null)
            {
                this.LoadData(scope);
            }

            this.State = ModuleStatus.Activated;
            LoggerProvider.PlatformLogger.Info(() => string.Format("Module [{0}] has been loaded.", this.Name));
        }


        private void LoadCoreModule(IServiceScope scope)
        {
            Debug.Assert(scope != null);

            var dbProfile = Platform.DBProfiles.TryGetDBProfile(scope.Session);

            var a = typeof(ObjectServer.Core.ModuleModel).Assembly;
            RegisterResourceWithinAssembly(dbProfile, a);

            dbProfile.InitializeAllResources();

            LoggerProvider.PlatformLogger.Info(() => "Importing data for the Core Module...");
            var importer = new Model.XmlDataImporter(scope, this.Name);
            foreach (var resPath in s_coreDataFiles)
            {
                using (var resStream = a.GetManifestResourceStream(resPath))
                {
                    LoggerProvider.PlatformLogger.Info(() => "Importing data file: [" + resPath + "]");
                    importer.Import(resStream);
                    resStream.Close();
                }
            }
        }


        private void LoadData(IServiceScope scope)
        {
            Debug.Assert(scope != null);

            LoggerProvider.PlatformLogger.Info(() => "Importing data...");

            var importer = new Model.XmlDataImporter(scope, this.Name);
            foreach (var dataFile in this.DataFiles)
            {
                var dataFilePath = System.IO.Path.Combine(this.Path, dataFile);
                LoggerProvider.PlatformLogger.Info(() => "Importing data file: [" + dataFilePath + "]");
                importer.Import(dataFilePath);
            }
        }

        private void LoadDynamicAssembly(IResourceContainer resContainer, IResourceContainer resources)
        {
            Debug.Assert(resContainer != null);
            Debug.Assert(resources != null);

            var a = CompileSourceFiles(this.Path);
            this.AllAssemblies.Add(a);
            RegisterResourceWithinAssembly(resContainer, a);
        }

        private void RegisterResourceWithinAssembly(IResourceContainer resContainer, Assembly assembly)
        {
            Debug.Assert(resContainer != null);
            Debug.Assert(assembly != null);

            LoggerProvider.PlatformLogger.Info(() => string.Format(
                "开始注册 DLL 中包含的资源：[{0}]...", assembly.FullName));

            var types = GetStaticModelsFromAssembly(assembly);

            foreach (var t in types)
            {
                var res = AbstractResource.CreateStaticResourceInstance(t);
                res.Module = this.Name;
                this.resources.Add(res);
                resContainer.RegisterResource(res);
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

        private void LoadStaticAssemblies(IResourceContainer resContainer)
        {
            Debug.Assert(this.Dlls != null);

            foreach (var dll in this.Dlls)
            {
                var a = Assembly.LoadFile(dll);
                this.allAssembly.Add(a);
                RegisterResourceWithinAssembly(resContainer, a);
            }
        }

        [XmlIgnore]
        public ICollection<Assembly> AllAssemblies { get { return this.allAssembly; } }

        [XmlIgnore]
        public string Path { get; set; }

        [XmlIgnore]
        public ModuleStatus State { get; set; }

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

        public void AddToDatabase(IDBContext dbctx)
        {
            if (dbctx == null)
            {
                throw new ArgumentNullException("dbctx");
            }

            var state = "deactivated";
            if (this.AutoLoad)
            {
                state = "activated";
            }

            var insertSql = SqlString.Parse("insert into core_module(name, state, info) values(?, ?, ?)");
            dbctx.Execute(insertSql, this.Name, state, this.Label);
        }

        public static Module CoreModule { get { return s_coreModule; } }

        private Assembly CompileSourceFiles(string moduleDir)
        {
            Debug.Assert(!string.IsNullOrEmpty(moduleDir));

            LoggerProvider.PlatformLogger.Info(String.Format(
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
            LoggerProvider.PlatformLogger.Info(String.Format("Elapsed time: [{0}]", time));
            LoggerProvider.PlatformLogger.Info(String.Format(
                "The module [{0}] has been compiled successfully.", this.Name));
            return a;
        }

    }
}
