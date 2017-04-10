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
            Fields.Chars("name").WithSize(64).WithLabel("Name").WithRequired();
            Fields.Enumeration("type", new Dictionary<string, string>()
            {
                { "integer", "Integer" },
                { "double", "Double"},
                { "binary", "Binary"},
                { "datetime", "Date Time"},
                { "text", "Text"},
            }).WithLabel("Type").WithRequired();

            Fields.Binary("value_binary").NotReadonly().WithLabel("Binary Value");
            Fields.Double("value_float").NotReadonly().WithLabel("Double Float Value");
            Fields.BigInteger("value_integer").NotReadonly().WithLabel("Integer Value");
            Fields.DateTime("value_datetime").NotReadonly().WithLabel("Datetime Value");
            Fields.Text("value_text").NotReadonly().WithLabel("Text Value");
            Fields.ManyToOne("organization", "core.organization").WithLabel("Organization");
        }

    }
}
