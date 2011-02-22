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
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        public const string ModuleFileName = "module.js";

        /// <summary>
        /// 整个系统中发现的所有模块
        /// </summary>
        private List<Module> allModules = new List<Module>();

        public ModulePool()
        {
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

                if (Log.IsInfoEnabled)
                {
                    Log.InfoFormat("Found module: [{0}], Path='{1}'",
                        module.Name, module.Path);
                }
            }

            DependencySort(modules);
        }

        private void DependencySort(List<Module> modules)
        {
            var sortedModules =
                DependencySorter.Sort(modules, m => m.Name, m => m.Depends);
            this.allModules.Clear();
            this.allModules.Capacity = sortedModules.Length;
            this.allModules.AddRange(sortedModules);
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateModuleList(IDatabaseContext db)
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


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void LoadModules(IDatabaseContext db, ObjectPool pool)
        {
            //加载的策略是：
            //只加载存在于文件系统，且数据库中设置为 state = 'actived' 的
            lock (typeof(Module))
            {
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

        private static void AddModuleToDb(IDatabaseContext db, Module m)
        {
            var state = "deactived";
            if (m.AutoLoad)
            {
                state = "actived";
            }

            var insertSql = "insert into core_module(name, state, info) values(@0, @1, @2)";
            db.Execute(insertSql, m.Name, state, m.Label);
        }
    }
}
