using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Core
{
    [Resource]
    public class FieldModel : AbstractSqlModel
    {
        public const string ModelName = "core.field";
        public const int FieldNameMax = 64;

        public FieldModel()
            : base(ModelName)
        {
            this.IsVersioned = false;

            Fields.ManyToOne("model", "core.model").WithLabel("Model")
                .WithRequired().OnDelete(OnDeleteAction.Cascade);
            Fields.Chars("name").WithLabel("Name").WithSize(64).WithRequired();
            Fields.Chars("label").WithLabel("Label").WithSize(256).WithNotRequired();
            Fields.Boolean("required").WithLabel("Required").WithRequired();
            Fields.Boolean("readonly").WithLabel("Read Only").WithRequired();
            Fields.Chars("relation").WithLabel("Relation").WithSize(256).WithNotRequired();
            Fields.Chars("type").WithLabel("Type").WithSize(32).WithRequired();
            Fields.Chars("help").WithLabel("Help").WithSize(256).WithNotRequired();
        }

        //TODO 拦截 WriteInternal DeleteInternal CreateInternal

    }
}
