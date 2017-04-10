using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;

using SlipStream.Entity;

namespace SlipStream.Core
{
    [Resource]
    public class MetaEntityEntity : AbstractSqlEntity
    {
        public const string EntityName = "core.meta_entity";

        public MetaEntityEntity()
            : base(EntityName)
        {
            this.IsVersioned = false;

            Fields.Chars("name").WithLabel("Name").WithSize(256).WithRequired().WithUnique().WithReadonly();
            Fields.Chars("label").WithLabel("Label").WithSize(256);
            Fields.Text("info").WithLabel("Information");
            Fields.Chars("module").WithLabel("Module").WithSize(128).WithRequired();
            Fields.OneToMany("fields", MetaFieldEntity.EntityName, "meta_entity").WithLabel("Fields");
        }

    }
}
