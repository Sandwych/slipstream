using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Core
{

    [Resource]
    public sealed class ActionModel : AbstractSqlModel
    {
        public ActionModel()
            : base("core.action")
        {
            Fields.Chars("name").WithLabel("Action Name").WithRequired();
            Fields.Chars("type").WithLabel("Action Type").WithRequired();
        }

    }
}
