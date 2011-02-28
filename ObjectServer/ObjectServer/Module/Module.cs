using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;
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

        [XmlElement("source-files")]
        public string[] SourceFiles { get; set; }

        [XmlArray("data-files")]
        [XmlArrayItem("file")]
        public string[] DataFiles { get; set; }

        [XmlElement("script-language")]
        public string ScriptLanguage { get; set; }

        [XmlElement("auto-load")]
        public bool AutoLoad { get; set; }

        [XmlArray("depends")]
        [XmlArrayItem("depend")]
        public string[] Depends { get; set; }

        [XmlArray("dlls")]
        [XmlArrayItem("file")]
        public string[] Dlls { get; set; }

        #endregion

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Load(IServiceContainer pool)
        {
            Logger.Info(() => string.Format("Loading module: '{0}'", this.Name));

            if (this.Dlls != null)
            {
                this.LoadStaticAssembly();
            }

            if (!string.IsNullOrEmpty(this.Path))
            {
                this.LoadDynamicAssembly(pool);
            }

            this.State = ModuleStatus.Activated;
            Logger.Info(() => string.Format("Module '{0}' has been loaded.", this.Name));
        }

        private void LoadDynamicAssembly(IServiceContainer pool)
        {
            Debug.Assert(pool != null);

            var a = CompileSourceFiles(this.Path);
            this.AllAssemblies.Add(a);
            RegisterServiceObjectWithinAssembly(pool, a);
        }

        private static void RegisterServiceObjectWithinAssembly(IServiceContainer objs, Assembly assembly)
        {
            Debug.Assert(objs != null);
            Debug.Assert(assembly != null);

            Logger.Info(() => string.Format(
                "Start to register all models for assembly [{0}]...", assembly.FullName));

            var types = GetStaticModelsFromAssembly(assembly);

            foreach (var t in types)
            {
                var obj = ObjectServiceBase.CreateStaticObjectInstance(t);
                objs.RegisterObject(obj);
            }
        }

        private static Type[] GetStaticModelsFromAssembly(Assembly assembly)
        {
            Debug.Assert(assembly != null);

            var types = assembly.GetTypes();
            var result = new List<Type>();
            foreach (var t in types)
            {
                var assemblies = t.GetCustomAttributes(typeof(ServiceObjectAttribute), false);
                if (assemblies.Length > 0)
                {
                    result.Add(t);
                }
            }
            return result.ToArray();
        }

        private void LoadStaticAssembly()
        {
            Debug.Assert(this.Dlls != null);

            foreach (var dll in this.Dlls)
            {
                var a = Assembly.LoadFile(dll);
                this.allAssembly.Add(a);
            }
        }

        [XmlIgnore]
        public ICollection<Assembly> AllAssemblies { get { return this.allAssembly; } }

        [XmlIgnore]
        public string Path { get; set; }

        [XmlIgnore]
        public ModuleStatus State { get; set; }

        public static Module DeserializeFromFile(string moduleFilePath)
        {
            var xs = new XmlSerializer(typeof(Module));

            using (var fs = File.OpenRead(moduleFilePath))
            {
                var module = (Module)xs.Deserialize(fs);
                fs.Close();
                return module;
            }
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
            var compiler = CompilerProvider.GetCompiler(this.ScriptLanguage);
            var a = compiler.CompileFromFile(sourceFiles);
            return a;
        }


        internal static void RegisterAllCoreObjects(IServiceContainer pool)
        {
            Debug.Assert(pool != null);

            var a = typeof(ObjectServer.Core.ModuleModel).Assembly;
            RegisterServiceObjectWithinAssembly(pool, a);
        }
    }
}
