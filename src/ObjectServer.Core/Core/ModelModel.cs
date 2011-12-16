using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Data;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public class ModelModel : AbstractSqlModel
    {
        public const string ModelName = "core.model";

        public ModelModel()
            : base(ModelName)
        {
            this.IsVersioned = false;

            Fields.Chars("name").SetLabel("Name").SetSize(256).Required().Unique().Readonly();
            Fields.Chars("label").SetLabel("Label").SetSize(256);
            Fields.Text("info").SetLabel("Information");
            Fields.Chars("module").SetLabel("Module").SetSize(128).Required();
            Fields.OneToMany("fields", "core.field", "model").SetLabel("Fields");
        }

    }
}
