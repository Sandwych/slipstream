using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

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
    [JsonObject("module")]
    public sealed class Module
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<Assembly> assembly = new List<Assembly>();
        private static readonly Module s_coreModule;

        static Module()
        {
            s_coreModule = new Module()
            {
                Name = StaticSettings.CoreModuleName,
                State = ModuleStatus.Actived,
                Depends = new string[] { },
            };
        }

        public Module()
        {
            //设置属性默认值
            this.Depends = new string[] { };
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

        [JsonProperty("depends")]
        public string[] Depends { get; set; }

        #endregion

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Load(ObjectPool pool)
        {
            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Begin to load module: '{0}'", this.Name);
            }

            var a = CompileSourceFiles(this.Path);
            this.Assemblies.Add(a);
            pool.AddModelsWithinAssembly(a);

            var moduleModel = (ModuleModel)pool[ModuleModel.ModelName];
            this.State = ModuleStatus.Actived;
            moduleModel.LoadedModules.Add(this);

            if (Log.IsInfoEnabled)
            {
                Log.InfoFormat("Module '{0}' has been loaded.", this.Name);
            }
        }

        [JsonIgnore]
        public ICollection<Assembly> Assemblies { get { return this.assembly; } }

        [JsonIgnore]
        public string Path { get; set; }

        [JsonIgnore]
        public ModuleStatus State { get; set; }

        public static Module DeserializeFromFile(string moduleFilePath)
        {
            var json = File.ReadAllText(moduleFilePath, Encoding.UTF8);
            var module = JsonConvert.DeserializeObject<Module>(json);
            return module;
        }

        public void AddToDatabase(IDatabaseContext db)
        {
            var state = "deactived";
            if (this.AutoLoad)
            {
                state = "actived";
            }

            var insertSql = "insert into core_module(name, state, info) values(@0, @1, @2)";
            db.Execute(insertSql, this.Name, state, this.Label);
        }

        public static Module CoreModule
        {
            get { return s_coreModule; }
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
            var a = compiler.CompileFromFile(sourceFiles);
            return a;
        }
    }
}
