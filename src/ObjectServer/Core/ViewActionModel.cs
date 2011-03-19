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
            Inherit("core.action", "action");
            Fields.ManyToOne("action", "core.action")
                .SetLabel("Base Action").Required().OnDelete(OnDeleteAction.Cascade);
            Fields.ManyToOne("primary_view", "core.view").SetLabel("Primary View").Required();           

        }

    }
}
