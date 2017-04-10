using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using SlipStream.Model;
using SlipStream.Runtime;
using SlipStream.Data;

namespace SlipStream.Core
{
    /// <summary>
    /// TODO 线程安全
    /// </summary>
    [Resource]
    public sealed class ModuleDependencyModel : AbstractSqlModel
    {
        public const string ModelName = "core.module_dependency";
        /// <summary>
        /// 单个数据库中加载的模块
        /// </summary>
        private readonly static Dictionary<string, string> StateOptions = new Dictionary<string, string>()
        {
            { ModuleModel.States.Installed, "Installed" },
            { ModuleModel.States.ToInstall, "To Install" },
            { ModuleModel.States.ToUninstall, "To Uninstall" },
            { ModuleModel.States.Uninstalled, "Uninstalled" },
            { ModuleModel.States.ToUpgrade, "To Upgrade" },
            { "unknown", "Unknown" },
        };

        public ModuleDependencyModel()
            : base(ModelName)
        {
            this.IsVersioned = false;

            Fields.Chars("name").WithLabel("Name").WithRequired().WithSize(128).WithUnique().Readonly();
            Fields.ManyToOne("module", "core.module").WithLabel("Module").WithRequired().Readonly();
            Fields.Enumeration("state", StateOptions).WithLabel("State").WithRequired().Readonly()
                .SetValueGetter(StateGetter);

        }

        private static Dictionary<long, object> StateGetter(IServiceContext tc, long[] ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            var selfModel = (IModel)tc.GetResource(ModelName);
            var moduleModel = (IModel)tc.GetResource("core.module");
            var result = new Dictionary<long, object>(ids.Length);
            var constraints = new object[][] { new object[] { "name", "=", null } };
            foreach (var depId in ids)
            {
                dynamic dep = selfModel.Browse(depId);
                constraints[0][2] = dep.name;
                var moduleIds = moduleModel.SearchInternal(constraints, null, 0, 0);

                if (moduleIds.Length > 0)
                {
                    result[depId] = moduleModel.Browse(moduleIds.First()).state;
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
