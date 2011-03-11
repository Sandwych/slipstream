using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Xml.Serialization;

using ObjectServer.Runtime;
using ObjectServer.Backend;
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

        public void Load(IResourceScope ctx)
        {
            this.resources.Clear();

            if (this.Name == "core")
            {
                this.LoadCoreModule(ctx);
            }
            else
            {
                this.LoadAdditionalModule(ctx);
            }
        }

        private void LoadAdditionalModule(IResourceScope ctx)
        {
            Logger.Info(() => string.Format("Loading module: '{0}'", this.Name));
            Logger.Info(() => "Loading program...");

            if (this.Dlls != null)
            {
                this.LoadStaticAssemblies(ctx.DatabaseProfile);
            }

            if (!string.IsNullOrEmpty(this.Path))
            {
                this.LoadDynamicAssembly(ctx.DatabaseProfile, ctx.DatabaseProfile);
            }

            ctx.DatabaseProfile.InitializeAllResources();

            if (this.DataFiles != null)
            {
                this.LoadData(ctx);
            }

            this.State = ModuleStatus.Activated;
            Logger.Info(() => string.Format("Module '{0}' has been loaded.", this.Name));
        }


        private void LoadCoreModule(IResourceScope ctx)
        {
            var a = typeof(ObjectServer.Core.ModuleModel).Assembly;
            RegisterResourceWithinAssembly(ctx.DatabaseProfile, a);

            ctx.DatabaseProfile.InitializeAllResources();

            Logger.Info(() => "Importing core data...");
            var importer = new Model.XmlDataImporter(ctx, this.Name);
            foreach (var resPath in s_coreDataFiles)
            {
                using (var resStream = a.GetManifestResourceStream(resPath))
                {
                    Logger.Info(() => "Importing file: " + resPath);
                    importer.Import(resStream);

                    resStream.Close();
                }
            }
        }


        private void LoadData(IResourceScope ctx)
        {
            Logger.Info(() => "Importing data...");

            var importer = new Model.XmlDataImporter(ctx, this.Name);
            foreach (var dataFile in this.DataFiles)
            {
                var dataFilePath = System.IO.Path.Combine(this.Path, dataFile);
                importer.Import(dataFilePath);
            }
        }

        private void LoadDynamicAssembly(IDatabaseProfile db, IResourceContainer resources)
        {
            Debug.Assert(resources != null);

            var a = CompileSourceFiles(this.Path);
            this.AllAssemblies.Add(a);
            RegisterResourceWithinAssembly(db, a);
        }

        private void RegisterResourceWithinAssembly(IDatabaseProfile db, Assembly assembly)
        {
            Debug.Assert(db != null);
            Debug.Assert(assembly != null);

            Logger.Info(() => string.Format(
                "Start to register all models for assembly [{0}]...", assembly.FullName));

            var types = GetStaticModelsFromAssembly(assembly);

            foreach (var t in types)
            {
                var res = AbstractResource.CreateStaticResourceInstance(t);
                this.resources.Add(res);
                db.RegisterResource(res);
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

        private void LoadStaticAssemblies(IDatabaseProfile db)
        {
            Debug.Assert(this.Dlls != null);

            foreach (var dll in this.Dlls)
            {
                var a = Assembly.LoadFile(dll);
                this.allAssembly.Add(a);
                RegisterResourceWithinAssembly(db, a);
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

        public void AddToDatabase(IDataContext db)
        {
            var state = "deactivated";
            if (this.AutoLoad)
            {
                state = "activated";
            }

            var insertSql = "insert into core_module(name, state, info) values(@0, @1, @2)";
            db.Execute(insertSql, this.Name, state, this.Label);
        }

        public static Module CoreModule { get { return s_coreModule; } }

        private Assembly CompileSourceFiles(string moduleDir)
        {
            Debug.Assert(!string.IsNullOrEmpty(moduleDir));

            var sourceFiles = new List<string>();
            foreach (var file in this.SourceFiles)
            {
                sourceFiles.Add(System.IO.Path.Combine(moduleDir, file));
            }

            //编译模块程序并注册所有对象
            var compiler = CompilerProvider.GetCompiler(this.SourceLanguage);
            var a = compiler.CompileFromFile(sourceFiles);
            return a;
        }

    }
}
