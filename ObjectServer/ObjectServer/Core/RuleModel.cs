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
            Fields.OneToMany("lines", "core.rule.line", "rule").SetLabel("Rule Details");
        }

    }

    [Resource]
    public sealed class RuleLineModel : AbstractTableModel
    {

        public RuleLineModel()
            : base("core.rule.line")
        {

            Fields.ManyToOne("rule", "core.rule").SetLabel("Creator");
        }
    }
}
