using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;

using ObjectServer.Utility;
using ObjectServer.Model;

namespace ObjectServer.Core
{
    [ServiceObject]
    public class ModelModel : TableModel
    {
        public ModelModel()
            : base()
        {
            this.Name = "core.model";
            this.Versioned = false;

            this.CharsField("name", "Name", 256, true, null, null);
            this.CharsField("label", "Label", 256, false, null, null);
            this.TextField("info", "Information", false, null, null);
            this.CharsField("module", "Module", 128, true, null, null);
            this.OneToManyField("fields", "core.field", "module", "Fields", false, null, null);
        }
    }
}
