using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [Resource]
    public sealed class WindowActionModel : AbstractTableModel
    {

        public WindowActionModel()
            : base("core.action_window")
        {
            Inherit("core.action", "action");
            Fields.ManyToOne("action", "core.action")
                .SetLabel("Base Action").Required().OnDelete(OnDeleteAction.Cascade);

            Fields.Chars("model").SetLabel("Related Model").Required().SetSize(128);
            Fields.ManyToOne("view", "core.view").SetLabel("Master View");
            Fields.OneToMany("views", "core.action_window_view", "window_action")
                .SetLabel("Views");
        }

    }
}
