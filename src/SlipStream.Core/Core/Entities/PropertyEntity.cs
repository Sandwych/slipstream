using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Core
{
    [Resource]
    public sealed class PropertyEntity : AbstractSqlEntity
    {

        public PropertyEntity() : base("core.property")
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

            Fields.Binary("value_binary").WithNotReadonly().WithLabel("Binary Value");
            Fields.Double("value_float").WithNotReadonly().WithLabel("Double Float Value");
            Fields.BigInteger("value_integer").WithNotReadonly().WithLabel("Integer Value");
            Fields.DateTime("value_datetime").WithNotReadonly().WithLabel("Datetime Value");
            Fields.Text("value_text").WithNotReadonly().WithLabel("Text Value");
            Fields.ManyToOne("organization", "core.organization").WithLabel("Organization");
        }

    }
}
