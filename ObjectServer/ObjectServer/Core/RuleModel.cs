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
        }

    }

    [ServiceObject]
    public sealed class RuleLineModel : TableModel
    {

        public RuleLineModel()
        {
            this.Name = "core.rule.line";
        }
    }
}
