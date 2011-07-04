using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [Resource]
    public sealed class WindowActionViewModel : AbstractTableModel
    {

        public WindowActionViewModel()
            : base("core.action_window_view")
        {
            Fields.Integer("sequence").Required().SetLabel("Sequence");
            Fields.ManyToOne("view", "core.view").SetLabel("Related View").Required();
            Fields.ManyToOne("window_action", "core.action_window")
                .SetLabel("Related Window Action").Required();
        }

    }
}
