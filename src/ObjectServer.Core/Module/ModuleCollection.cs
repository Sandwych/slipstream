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
            if (cfg == null)
            {
                throw new ArgumentNullException("cfg");
            }

            this.LookupAllModules(cfg.ModulePath);
        }

        #endregion


        public bool Contains(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new ArgumentNullException("moduleName");
            }

            return this.allModules.Exists(m => m.Name == moduleName);
        }


        public Module GetModule(string moduleName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new ArgumentNullException("moduleName");
            }

            Module result = this.allModules.SingleOrDefault(m => m.Name == moduleName);
            if (result == null)
            {
                var msg = string.Format("Cannot found module: [{0}]", moduleName);
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
            if (string.IsNullOrEmpty(modulePath))
            {
                throw new ArgumentNullException("modulePath");
            }

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

                LoggerProvider.PlatformLogger.Info(() => string.Format("Found additional module: [{0}], Path=[{1}]",
                        module.Name, module.Path));
            }

            modules.DependencySort(m => m.Name, m => m.Depends);
            this.allModules = modules;
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void UpdateModuleList(IDBContext dbctx)
        {
            if (dbctx == null)
            {
                throw new ArgumentNullException("dbctx");
            }

            var cfg = Environment.Configuration;
            this.LookupAllModules(cfg.ModulePath);

            var sql = new SqlString("select name from core_module");
            var moduleNames = new HashSet<string>(dbctx.QueryAsArray<string>(sql));

            foreach (var m in allModules)
            {
                if (!moduleNames.Contains(m.Name))
                {
                    m.AddToDatabase(dbctx);
                }
            }
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void LoadModules(IServiceScope scope)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }
            //加载的策略是：
            //1. 加载状态为 "installed" 的模块
            //2. 加载状态为 "to-install" 的模块
            //SQL: select _id, name from core_module where state='activated'

            //加载已安装的模块
            var sql = new SqlString("select _id, name, state from core_module");

            var modules = scope.DBContext.QueryAsDictionary(sql, ModuleModel.States.Installed);

            foreach (var m in modules)
            {
                var moduleName = (string)m["name"];
                var module = this.allModules.SingleOrDefault(i => i.Name == moduleName);
                var moduleId = (long)m[AbstractModel.IDFieldName];
                if (module != null)
                {
                    var state = (string)m["state"];
                    if (state == ModuleModel.States.Installed)
                    {
                        module.Load(scope, false);
                    }
                    else if (state == ModuleModel.States.ToInstall
                        || state == ModuleModel.States.ToUpgrade)
                    {
                        module.Load(scope, true);
                        this.UpdateModuleState(scope.DBContext, moduleId, ModuleModel.States.Installed);
                    }
                    else if (state == ModuleModel.States.ToUninstall)
                    {
                        this.UpdateModuleState(scope.DBContext, moduleId, ModuleModel.States.Uninstalled);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    this.UpdateModuleState(scope.DBContext, moduleId, ModuleModel.States.Uninstalled);
                    LoggerProvider.PlatformLogger.Warn(() => string.Format(
                        "Warning: Cannot found module '{0}', it will be deactivated.", moduleName));
                }
            }

        }

        private void UpdateModuleState(IDBContext db, long moduleID, string state)
        {
            Debug.Assert(moduleID > 0);
            Debug.Assert(db != null);
            Debug.Assert(!string.IsNullOrEmpty(state));

            var sql = new SqlString("update core_module set state=", Parameter.Placeholder,
                " where _id=", Parameter.Placeholder);
            db.Execute(sql, state, moduleID);
        }

    }
}
