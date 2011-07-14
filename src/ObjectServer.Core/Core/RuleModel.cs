using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class RuleModel : AbstractTableModel
    {
        public RuleModel()
            : base("core.rule")
        {
            Fields.Chars("name").SetLabel("Name").Required();
            Fields.ManyToOne("model", "core.model").Required().SetLabel("Model");
            Fields.Chars("domain").SetLabel("Domain").Required();
            Fields.Boolean("global").SetLabel("Global")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("on_create").SetLabel("Apply for Creation")
               .Required().DefaultValueGetter(s => true);
            Fields.Boolean("on_read").SetLabel("Apply for Reading")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("on_write").SetLabel("Apply for Writing")
                .Required().DefaultValueGetter(s => true);
            Fields.Boolean("on_delete").SetLabel("Apply for Deleting")
                .Required().DefaultValueGetter(s => true);
            Fields.ManyToMany("groups", "core.user_group", "rid", "gid").SetLabel("Groups");
        }

    }
}
