using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;

using SlipStream.Model;

namespace SlipStream.Core
{
    [Resource]
    public class ModelModel : AbstractSqlModel
    {
        public const string ModelName = "core.model";

        public ModelModel()
            : base(ModelName)
        {
            this.IsVersioned = false;

            Fields.Chars("name").WithLabel("Name").WithSize(256).WithRequired().WithUnique().Readonly();
            Fields.Chars("label").WithLabel("Label").WithSize(256);
            Fields.Text("info").WithLabel("Information");
            Fields.Chars("module").WithLabel("Module").WithSize(128).WithRequired();
            Fields.OneToMany("fields", "core.field", "model").WithLabel("Fields");
        }

    }
}
