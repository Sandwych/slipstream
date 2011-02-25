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

namespace ObjectServer.Core
{
    /// <summary>
    /// TODO 线程安全
    /// </summary>
    [ServiceObject]
    public sealed class ModuleModel : TableModel
    {
        public const string ModelName = "core.module";
        /// <summary>
        /// 单个数据库中加载的模块
        /// </summary>
        private readonly List<Module> loadedModules
            = new List<Module>();

        public ModuleModel()
            : base(ModelName)
        {
            Fields.Chars("name").SetLabel("Name").SetRequired().SetSize(128);
            Fields.Chars("state").SetLabel("State").SetRequired().SetSize(16);
            Fields.Text("info").SetLabel("Information");
        }

        public List<Module> LoadedModules
        {
            get { return this.loadedModules; }
        }

    }
}
