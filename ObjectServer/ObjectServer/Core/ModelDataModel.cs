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

            Fields.Chars("name").SetLabel("Name(Friendly Ref. ID)").SetRequired().SetSize(128);
            Fields.Chars("module").SetLabel("Module").SetRequired().SetSize(64);
            Fields.Chars("model").SetLabel("Model").SetRequired().SetSize(64);
            Fields.BigInteger("ref_id").SetLabel("Referenced ID").SetRequired();
            Fields.Text("value").SetLabel("Value");
        }
    }
}
