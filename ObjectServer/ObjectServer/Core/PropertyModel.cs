using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class PropertyModel : TableModel
    {

        public PropertyModel()
            : base("core.property")
        {
            Fields.Chars("name").SetSize(64).SetLabel("Name").Required();
            Fields.Enumeration("type", new Dictionary<string, string>()
            {
                { "integer", "Integer" },
                { "float", "Float"},
                { "binary", "Binary"},
                { "datetime", "Date Time"},
                { "text", "Text"},
            }).SetLabel("Type").Required();

            Fields.Binary("value_binary").NotReadonly().SetLabel("Binary Value");
            Fields.Float("value_float").NotReadonly().SetLabel("Float Value");
            Fields.BigInteger("value_integer").NotReadonly().SetLabel("Integer Value");
            Fields.DateTime("value_datetime").NotReadonly().SetLabel("Datetime Value");
            Fields.Text("value_text").NotReadonly().SetLabel("Text Value");
        }

    }
}
