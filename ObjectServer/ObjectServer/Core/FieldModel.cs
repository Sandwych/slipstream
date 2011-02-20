using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [ServiceObject]
    public class FieldModel : TableModel
    {

        public FieldModel()
            : base()
        {
            this.Name = "core.field";

            this.ManyToOneField("model", "core.model", "Model", true, null, null);
            this.CharsField("name", "Name", 64, true, null, null);
            this.CharsField("label", "Label", 256, false, null, null);
            this.CharsField("type", "Type", 32, true, null, null);
            this.CharsField("help", "Help", 256, false, null, null);
        }


    }
}
