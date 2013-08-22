using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Core
{
    [Resource]
    public sealed class PropertyModel : AbstractSqlModel
    {

        public PropertyModel()
            : base("core.property")
        {
            Fields.Chars("name").SetSize(64).SetLabel("Name").Required();
            Fields.Enumeration("type", new Dictionary<string, string>()
            {
                { "integer", "Integer" },
                { "double", "Double"},
                { "binary", "Binary"},
                { "datetime", "Date Time"},
                { "text", "Text"},
            }).SetLabel("Type").Required();

            Fields.Binary("value_binary").NotReadonly().SetLabel("Binary Value");
            Fields.Double("value_float").NotReadonly().SetLabel("Double Float Value");
            Fields.BigInteger("value_integer").NotReadonly().SetLabel("Integer Value");
            Fields.DateTime("value_datetime").NotReadonly().SetLabel("Datetime Value");
            Fields.Text("value_text").NotReadonly().SetLabel("Text Value");
            Fields.ManyToOne("organization", "core.organization").SetLabel("Organization");
        }

    }
}
