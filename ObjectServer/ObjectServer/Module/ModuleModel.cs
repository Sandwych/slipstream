using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using Newtonsoft.Json;

using ObjectServer.Model;
using ObjectServer.Runtime;
using ObjectServer.Backend;

namespace ObjectServer.Module
{
    /// <summary>
    /// TODO 线程安全
    /// </summary>
    [ServiceObject]
    public sealed class ModuleModel : ModelBase
    {
        /// <summary>
        /// 单个数据库中加载的模块
        /// </summary>
        private readonly List<Module> loadedModules
            = new List<Module>();

        public ModuleModel()
        {
            this.Name = "core.module";
            this.Automatic = false;
            this.Versioned = false;

            this.CharsField("name", "Name", 128, true, null);
            this.CharsField("state", "State", 16, true, null);
            this.TextField("description", "Description", false, null);
        }

        public static void LoadModules(IDatabase db, string dbName, ObjectPool pool)
        {
            var sql = "select name from core_module where state = 'installed'";
            var modules = db.QueryAsDictionary(sql);

            foreach (var m in modules)
            {
                LoadModule(dbName, db, (string)m["name"], pool);
            }
        }

        /// <summary>
        /// 注册模块
        /// TODO 线程安全
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="conn"></param>
        /// <param name="module"></param>
        /// <param name="pool"></param>
        private static void LoadModule(string dbName, IDatabase db, string module, ObjectPool pool)
        {
            var moduleDir = Path.Combine(@"c:\objectserver-modules", module);
            var moduleFilePath = Path.Combine(moduleDir, "module.js");

            Module moduleInfo;
            var json = File.ReadAllText(moduleFilePath, Encoding.UTF8);

            moduleInfo = JsonConvert.DeserializeObject<Module>(json);

            var assembly = CompileSourceFiles(moduleDir, moduleInfo);
            moduleInfo.Assemblies.Add(assembly);
            pool.RegisterModelsInAssembly(assembly);
        }

        private static System.Reflection.Assembly CompileSourceFiles(
            string moduleDir, Module moduleInfo)
        {
            var sourceFiles = new List<string>();
            foreach (var file in moduleInfo.SourceFiles)
            {
                sourceFiles.Add(Path.Combine(moduleDir, file));
            }

            //编译模块程序并注册所有对象
            var compiler = CompilerProvider.GetCompiler(moduleInfo.ScriptLanguage);
            var assembly = compiler.CompileFromFile(sourceFiles);
            return assembly;
        }

    }
}
