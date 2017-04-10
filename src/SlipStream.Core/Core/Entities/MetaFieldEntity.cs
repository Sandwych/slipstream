using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Core
{
    [Resource]
    public class MetaFieldEntity : AbstractSqlEntity
    {
        public const string EntityName = "core.meta_field";
        public const int FieldNameMax = 64;

        public MetaFieldEntity()
            : base(EntityName)
        {
            this.IsVersioned = false;

            Fields.ManyToOne("meta_entity", MetaEntityEntity.EntityName).WithLabel("Meta Entity")
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
