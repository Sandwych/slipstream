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
    public sealed class ModulePool
    {
        public const string ModuleFileName = "module.js";

        /// <summary>
        /// 整个系统中发现的所有模块
        /// </summary>
        private List<Module> allModules = new List<Module>();

        public ModulePool()
        {
            this.allModules.Add(Module.CoreModule);
        }


        public bool Contains(string moduleName)
        {
            return this.allModules.Exists(m => m.Name == moduleName);
        }


        public Module GetModule(string moduleName)
        {
            Module result = this.allModules.SingleOrDefault(m => m.Name == moduleName);
            if (result == null)
            {
                var msg = string.Format("Cannot found module: '{0}'", moduleName);
                throw new ModuleNotFoundException(msg, moduleName);
            }
            else
            {
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void LookupAllModules(string modulePath)
        {
            var modules = new List<Module>();
            modules.Add(Module.CoreModule);

            if (string.IsNullOrEmpty(modulePath) || !Directory.Exists(modulePath))
            {
                return;
            }

            var moduleDirs = Directory.GetDirectories(modulePath);
            foreach (var moduleDir in moduleDirs)
            {
                var moduleFilePath = System.IO.Path.Combine(moduleDir, ModuleFileName);
                var module = Module.DeserializeFromFile(moduleFilePath);
                module.Path = moduleDir;
                modules.Add(module);

                Logger.Info(() => string.Format("Found module: [{0}], Path='{1}'",
                        module.Name, module.Path));
            }

            modules.DependencySort(m => m.Name, m => m.Depends);
            this.allModules = modules;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateModuleList(IDatabaseContext db)
        {
            var cfg = ObjectServerStarter.Configuration;
            this.LookupAllModules(cfg.ModulePath);

            var sql = "select count(*) from core_module where name = @0";

            foreach (var m in allModules)
            {
                var count = (long)db.QueryValue(sql, m.Name);
                if (count == 0)
                {
                    m.AddToDatabase(db);
                }
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void LoadActivedModules(IDatabaseContext db, ObjectPool pool)
        {
            //加载的策略是：
            //只加载存在于文件系统，且数据库中设置为 state = 'actived' 的
            var sql = "select name from core_module where state = 'actived'";
            var modules = db.QueryAsDictionary(sql);

            foreach (var m in modules)
            {
                var moduleName = (string)m["name"];
                var module = this.allModules.SingleOrDefault(i => i.Name == moduleName);
                if (module != null)
                {
                    module.Load(pool);
                }
                else
                {
                    var msg =
                        string.Format("Cannot found module: '{0}'", moduleName);
                    throw new ModuleNotFoundException(msg, moduleName);
                }
            }
        }

    }
}
