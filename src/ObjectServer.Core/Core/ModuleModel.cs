using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

using ObjectServer.Model;
using ObjectServer.Runtime;
using ObjectServer.Backend;

namespace ObjectServer.Core
{
    /// <summary>
    /// TODO 线程安全
    /// </summary>
    [Resource]
    public sealed class ModuleModel : AbstractTableModel
    {
        public const string ModelName = "core.module";
        /// <summary>
        /// 单个数据库中加载的模块
        /// </summary>

        public ModuleModel()
            : base(ModelName)
        {
            this.AutoMigration = false;

            Fields.Chars("name").SetLabel("Name").Required().SetSize(128).Unique();
            Fields.Chars("state").SetLabel("State").Required().SetSize(16);
            Fields.Text("info").SetLabel("Information");
        }

    }
}
