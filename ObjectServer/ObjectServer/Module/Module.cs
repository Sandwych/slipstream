using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

using ObjectServer.Runtime;

using Newtonsoft.Json;

namespace ObjectServer.Module
{
    /// <summary>
    /// TODO: 线程安全
    /// </summary>
    [Serializable]
    [JsonObject("module")]
    public sealed class Module
    {
        /// <summary>
        /// 整个系统中发现的所有模块
        /// </summary>
        private static readonly List<Module> allModules =
            new List<Module>();

        public Module()
        {
            this.Assemblies = new HashSet<Assembly>();
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
        public HashSet<Assembly> Assemblies { get; set; }

        [JsonIgnore]
        public string Path { get; set; }

        [JsonIgnore]
        public ModuleStatus State { get; set; }

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
