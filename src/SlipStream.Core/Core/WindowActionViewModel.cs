using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Core
{

    /// <summary>
    /// WindowAction 与 View 的关联表
    /// </summary>
    [Resource]
    public sealed class WindowActionViewModel : AbstractSqlModel
    {

        public WindowActionViewModel()
            : base("core.action_window_view")
        {
            Fields.Integer("ordinal").Required().SetLabel("Ordinal");
            Fields.ManyToOne("view", "core.view").SetLabel("Related View").Required();
            Fields.ManyToOne("window_action", "core.action_window")
                .SetLabel("Related Window Action").Required();
        }

    }
}
