using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using NHibernate.SqlCommand;

using ObjectServer.Exceptions;
using ObjectServer.Model;
using ObjectServer.Runtime;
using ObjectServer.Data;
using ObjectServer.Core;
using ObjectServer.Utility;

namespace ObjectServer
{
    /// <summary>
    /// </summary>
    [Serializable]
    public sealed class ModuleCollection : IGlobalObject
    {

        /// <summary>
        /// 整个系统中发现的所有模块
        /// </summary>
        private List<Module> allModules = new List<Module>();

        public ModuleCollection()
        {
            this.allModules.Add(Module.CoreModule);
        }


        #region IGlobalObject 成员

        public void Initialize(Config cfg)
        {
            this.LookupAllModules(cfg.ModulePath);
        }

        #endregion


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
                var moduleFilePath = System.IO.Path.Combine(
                    moduleDir, StaticSettings.ModuleMetaDataFileName);

                var module = Module.Deserialize(moduleFilePath);

                module.Path = moduleDir;
                modules.Add(module);

                Logger.Info(() => string.Format("Found module: [{0}], Path='{1}'",
                        module.Name, module.Path));
            }

            modules.DependencySort(m => m.Name, m => m.Depends);
            this.allModules = modules;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateModuleList(IDBConnection db)
        {
            var cfg = Platform.Configuration;
            this.LookupAllModules(cfg.ModulePath);

            var sql = new SqlString(
                "select count(*) from ",
                DataProvider.Dialect.QuoteForTableName("core_module"),
                " where ",
                DataProvider.Dialect.QuoteForColumnName("name"), "=", Parameter.Placeholder);

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
        public void LoadActivatedModules(IServiceScope ctx)
        {
            //加载的策略是：
            //只加载存在于文件系统，且数据库中设置为 state = 'activated' 的
            //SQL: select _id, name from core_module where state='activated'
            //TODO 加上引号
            var sql = new SqlString(
                " select _id, name from core_module where state=", Parameter.Placeholder);

            var modules = ctx.Connection.QueryAsDictionary(sql, "activated");

            var unloadModules = new List<long>();
            foreach (var m in modules)
            {
                var moduleName = (string)m["name"];
                var module = this.allModules.SingleOrDefault(i => i.Name == moduleName);
                if (module != null)
                {
                        module.Load(ctx);
                }
                else
                {
                    unloadModules.Add((long)m[AbstractModel.IDFieldName]);
                    Logger.Warn(() => string.Format(
                        "Warning: Cannot found module '{0}', it will be deactivated.", moduleName));
                }
            }

            if (unloadModules.Count > 0)
            {
                DeactivateModules(ctx.Connection, unloadModules);
            }
        }


        private static void DeactivateModules(IDBConnection db, IEnumerable<long> unloadedModuleIds)
        {
            Debug.Assert(unloadedModuleIds.Count() > 0);
            Debug.Assert(db != null);

            var ids = unloadedModuleIds.ToCommaList();
            var sql2 = string.Format(
                "update core_module set state = 'deactivated' where _id IN ({0})",
                ids);

            db.Execute(SqlString.Parse(sql2));
        }

    }
}
