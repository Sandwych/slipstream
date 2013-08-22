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
            Fields.Chars("name").SetLabel("Action Name").Required();
            Fields.Chars("type").SetLabel("Action Type").Required();
        }

    }
}
