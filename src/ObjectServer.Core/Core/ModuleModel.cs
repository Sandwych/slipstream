using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using ObjectServer.Model;
using ObjectServer.Runtime;
using ObjectServer.Data;

namespace ObjectServer.Core
{
    /// <summary>
    /// TODO 线程安全
    /// </summary>
    [Resource]
    public sealed class ModuleModel : AbstractTableModel
    {
        public static class States
        {
            public const string Uninstalled = "uninstalled";
            public const string Installed = "installed";
            public const string ToInstall = "to-install";
            public const string ToUpgrade = "to-upgrade";
            public const string ToUninstall = "to-uninstall";
        }

        public const string ModelName = "core.module";
        /// <summary>
        /// 单个数据库中加载的模块
        /// </summary>

        public ModuleModel()
            : base(ModelName)
        {
            this.AutoMigration = false;

            Fields.Chars("name").SetLabel("Name").Required().SetSize(128).Unique();
            Fields.Enumeration("state", new Dictionary<string, string>()
            {
                { States.Uninstalled, "Uninstalled" },
                { States.Installed, "Installed" },
                { States.ToInstall, "To Install" },
                { States.ToUpgrade, "To Upgrade" },
                { States.ToUninstall, "To Uninstall" },
            }).Required().SetLabel("State");
            Fields.Text("info").SetLabel("Information");
        }

    }
}
