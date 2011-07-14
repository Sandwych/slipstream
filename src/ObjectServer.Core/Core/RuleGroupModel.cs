using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class RuleGroupModel : AbstractTableModel
    {

        public RuleGroupModel()
            : base("core.rule_group")
        {
            this.TableName = "core_rule_group_rel";

            Fields.ManyToOne("rid", "core.rule").SetLabel("Rule").Required();
            Fields.ManyToOne("gid", "core.group").SetLabel("Group").Required();

        }
    }
}
