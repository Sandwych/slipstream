using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using Newtonsoft.Json;

using ObjectServer.Runtime;
using ObjectServer.Backend;

namespace ObjectServer.Module
{
    /// <summary>
    /// TODO: 线程安全
    /// </summary>
    [Serializable]
    [JsonObject("module")]
    public sealed class Module
    {
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

        [JsonProperty("source-files")]
        public string[] SourceFiles { get; set; }

        [JsonProperty("data-files")]
        public string[] DataFiles { get; set; }

        [JsonProperty("script-language")]
        public string ScriptLanguage { get; set; }

        [JsonProperty("auto-load")]
        public bool AutoLoad { get; set; }

        #endregion

        public void Load(ObjectPool pool)
        {
            var assembly = CompileSourceFiles(this.Path);
            this.Assemblies.Add(assembly);
            pool.RegisterModelsInAssembly(assembly);
        }

        [JsonIgnore]
        public List<Assembly> Assemblies { get; set; }

        [JsonIgnore]
        public string Path { get; set; }

        [JsonIgnore]
        public ModuleStatus State { get; set; }

        public static void LookupAllModules()
        {
            allModules.Clear();

            var path = ObjectServerStarter.Configuration.ModulePath;
            var moduleDirs = Directory.GetDirectories(path);
            foreach (var moduleDir in moduleDirs)
            {
                var moduleFilePath = System.IO.Path.Combine(path, moduleDir);
                moduleFilePath = System.IO.Path.Combine(moduleFilePath, MODULE_FILENAME);
                var module = DeserializeFromFile(moduleFilePath);
                allModules.Add(module);
            }
        }

        public static void LoadModules(IDatabase db, ObjectPool pool)
        {
            var sql = "select name from core_module where state = 'installed'";
            var modules = db.QueryAsDictionary(sql);

            foreach (var m in modules)
            {
                var moduleName = (string)m["name"];
                var module = allModules.Single(i => i.Name == moduleName);
                module.Load(pool);
            }
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
