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
    public sealed class ModelDataModel : TableModel
    {
        public const string ModelName = "core.model_data";

        public ModelDataModel()
        {
            this.Name = ModelName;

            this.CharsField("name", "Name(Friendly Ref. ID)", 128, true, null, null);
            this.CharsField("module", "Module", 64, true, null, null);
            this.CharsField("model", "Model", 64, true, null, null);
            this.BitIntegerField("ref_id", "Referenced ID", true, null, null);
            this.TextField("value", "Value", false, null, null);
        }
    }
}
