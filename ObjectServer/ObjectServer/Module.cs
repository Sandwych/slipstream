using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using Newtonsoft.Json;

using ObjectServer.Runtime;
using ObjectServer.Backend;
using ObjectServer.Core;

namespace ObjectServer
{
    /// <summary>
    /// TODO: 线程安全
    /// </summary>
    [Serializable]
    [JsonObject("module")]
    public sealed class Module
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        public const string MODULE_FILENAME = "module.js";

        /// <summary>
        /// 整个系统中发现的所有模块
        /// </summary>
        private static readonly List<Module> allModules =
            new List<Module>();

        public Module()
        {
            this.Assemblies = new List<Assembly>();
        }

        #region Serializable Fields
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("label", Required = Required.Default)]
        public string Label { get; set; }

        [JsonProperty("description", Required = Required.Default)]
        public string Description { get; set; }

        [JsonProperty("source_files")]
        public string[] SourceFiles { get; set; }

        [JsonProperty("data_files")]
        public string[] DataFiles { get; set; }

        [JsonProperty("script_language")]
        public string ScriptLanguage { get; set; }

        [JsonProperty("auto_load")]
        public bool AutoLoad { get; set; }

        #endregion

        public void Load(ObjectPool pool)
        {
            var assembly = CompileSourceFiles(this.Path);
            this.Assemblies.Add(assembly);
            pool.RegisterModelsInAssembly(assembly);

            var moduleModel = (ModuleModel)pool.LookupObject("core.module");
            this.State = ModuleStatus.Actived;
            moduleModel.LoadedModules.Add(this);
        }

        [JsonIgnore]
        public List<Assembly> Assemblies { get; set; }

        [JsonIgnore]
        public string Path { get; set; }

        [JsonIgnore]
        public ModuleStatus State { get; set; }

        public static void LookupAllModules(string modulePath)
        {
            lock (typeof(Module))
            {
                allModules.Clear();

                if (string.IsNullOrEmpty(modulePath) || !Directory.Exists(modulePath))
                {
                    return;
                }

                var moduleDirs = Directory.GetDirectories(modulePath);
                foreach (var moduleDir in moduleDirs)
                {
                    var moduleFilePath = System.IO.Path.Combine(moduleDir, MODULE_FILENAME);
                    var module = DeserializeFromFile(moduleFilePath);
                    module.Path = moduleDir;
                    allModules.Add(module);
                }
            }
        }

        public static void UpdateModuleList(IDatabase db)
        {
            lock (typeof(Module))
            {
                var sql = "select count(*) from core_module where name = @0";

                foreach (var m in allModules)
                {
                    var count = (long)db.QueryValue(sql, m.Name);
                    if (count == 0)
                    {
                        AddModuleToDb(db, m);
                    }
                }
            }
        }


        public static void LoadModules(IDatabase db, ObjectPool pool)
        {
            lock (typeof(Module))
            {
                var sql = "select name from core_module where state = 'actived'";
                var modules = db.QueryAsDictionary(sql);

                foreach (var m in modules)
                {
                    var moduleName = (string)m["name"];
                    var module = allModules.SingleOrDefault(i => i.Name == moduleName);
                    if (module != null)
                    {
                        module.Load(pool);
                    }
                    else
                    {
                        var msg = string.Format("Cannot found module: '{0}'", moduleName);
                        throw new FileNotFoundException(msg, moduleName);
                    }
                }
            }
        }


        private static void AddModuleToDb(IDatabase db, Module m)
        {
            var state = "deactived";
            if (m.AutoLoad)
            {
                state = "actived";
            }

            var insertSql = "insert into core_module(name, state, info) values(@0, @1, @2)";
            db.Execute(insertSql, m.Name, state, m.Label);
        }

        private static Module DeserializeFromFile(string moduleFilePath)
        {
            var json = File.ReadAllText(moduleFilePath, Encoding.UTF8);
            var module = JsonConvert.DeserializeObject<Module>(json);
            return module;
        }

        private Assembly CompileSourceFiles(string moduleDir)
        {
            var sourceFiles = new List<string>();
            foreach (var file in this.SourceFiles)
            {
                sourceFiles.Add(System.IO.Path.Combine(moduleDir, file));
            }

            //编译模块程序并注册所有对象
            var compiler = CompilerProvider.GetCompiler(this.ScriptLanguage);
            var assembly = compiler.CompileFromFile(sourceFiles);
            return assembly;
        }
    }
}
