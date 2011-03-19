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
            Fields.ManyToOne("group", "core.group").SetLabel("User Group").Required();
        }

    }
}
