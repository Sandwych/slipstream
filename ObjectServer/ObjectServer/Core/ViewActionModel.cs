using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [Resource]
    public sealed class ViewActionModel : AbstractTableModel
    {

        public ViewActionModel()
            : base("core.action_view")
        {
            Fields.ManyToOne("primary_view", "core.view").SetLabel("Primary View").Required();           
        }

    }
}
