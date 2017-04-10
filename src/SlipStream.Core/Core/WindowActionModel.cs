using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Core
{

    [Resource]
    public sealed class WindowActionModel : AbstractSqlModel
    {

        public WindowActionModel()
            : base("core.action_window")
        {
            Inherit("core.action", "action");
            Fields.ManyToOne("action", "core.action")
                .WithLabel("Base Action").WithRequired().OnDelete(OnDeleteAction.Cascade);

            Fields.Chars("model").WithLabel("Related Model").WithRequired().WithSize(128);
            Fields.ManyToOne("view", "core.view").WithLabel("Master View");
            Fields.OneToMany("views", "core.action_window_view", "window_action").WithLabel("Views");
        }

    }
}
