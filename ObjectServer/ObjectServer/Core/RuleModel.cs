using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [ServiceObject]
    public sealed class RuleModel : TableModel
    {
        public RuleModel()
        {
            this.Name = "core.rule";

            this.OneToManyField("lines", "core.rule.line", "rule", "Rule Details", false, null, null);
        }

    }

    [ServiceObject]
    public sealed class RuleLineModel : TableModel
    {

        public RuleLineModel()
        {
            this.Name = "core.rule.line";

            this.ManyToOneField("rule", "core.rule", "Creator", false, null, null);
        }
    }
}
