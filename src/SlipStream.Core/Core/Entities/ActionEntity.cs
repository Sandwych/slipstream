using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Core
{

    [Resource]
    public sealed class ActionEntity : AbstractSqlEntity
    {
        public ActionEntity() : base("core.action")
        {
            Fields.Chars("name").WithLabel("Action Name").WithRequired();
            Fields.Chars("type").WithLabel("Action Type").WithRequired();
        }

    }
}
