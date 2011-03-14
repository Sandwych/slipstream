using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [Resource]
    public sealed class ActionModel : AbstractTableModel
    {

        public ActionModel()
            : base("core.action")
        {
            Fields.Chars("name").SetLabel("Action Name").Required();
            Fields.Chars("kind").SetLabel("View Kind").Required();
        }

    }
}
