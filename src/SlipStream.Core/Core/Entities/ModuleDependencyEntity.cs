using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using SlipStream.Entity;
using SlipStream.Runtime;
using SlipStream.Data;

namespace SlipStream.Core
{
    /// <summary>
    /// TODO 线程安全
    /// </summary>
    [Resource]
    public sealed class ModuleDependencyEntity : AbstractSqlEntity
    {
        public const string EntityName = "core.module_dependency";
        /// <summary>
        /// 单个数据库中加载的模块
        /// </summary>
        private readonly static Dictionary<string, string> StateOptions = new Dictionary<string, string>()
        {
            { ModuleEntity.States.Installed, "Installed" },
            { ModuleEntity.States.ToInstall, "To Install" },
            { ModuleEntity.States.ToUninstall, "To Uninstall" },
            { ModuleEntity.States.Uninstalled, "Uninstalled" },
            { ModuleEntity.States.ToUpgrade, "To Upgrade" },
            { "unknown", "Unknown" },
        };

        public ModuleDependencyEntity()
            : base(EntityName)
        {
            this.IsVersioned = false;

            Fields.Chars("name").WithLabel("Name").WithRequired().WithSize(128).WithUnique().WithReadonly();
            Fields.ManyToOne("module", "core.module").WithLabel("Module").WithRequired().WithReadonly();
            Fields.Enumeration("state", StateOptions).WithLabel("State").WithRequired().WithReadonly()
                .WithValueGetter(StateGetter);

        }

        private static Dictionary<long, object> StateGetter(IServiceContext tc, long[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            var selfEntity = (IEntity)tc.GetResource(EntityName);
            var moduleEntity = (IEntity)tc.GetResource("core.module");
            var result = new Dictionary<long, object>(ids.Length);
            var constraints = new object[][] { new object[] { "name", "=", null } };
            foreach (var depId in ids)
            {
                dynamic dep = selfEntity.Browse(depId);
                constraints[0][2] = dep.name;
                var moduleIds = moduleEntity.SearchInternal(constraints, null, 0, 0);

                if (moduleIds.Length > 0)
                {
                    result[depId] = moduleEntity.Browse(moduleIds.First()).state;
                }
                else
                {
                    result[depId] = "unknown";
                }
            }

            return result;
        }

    }
}
