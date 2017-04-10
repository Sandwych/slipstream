using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Core
{

    [Resource]
    public sealed class WindowActionEntity : AbstractSqlEntity
    {

        public WindowActionEntity()
            : base("core.action_window")
        {
            Inherit("core.action", "action");
            Fields.ManyToOne("action", "core.action")
                .WithLabel("Base Action").WithRequired().OnDelete(OnDeleteAction.Cascade);

            Fields.Chars("entity").WithLabel("Related Entity").WithRequired().WithSize(128);
            Fields.ManyToOne("view", "core.view").WithLabel("Master View");
            Fields.OneToMany("views", "core.action_window_view", "window_action").WithLabel("Views");
        }

    }
}
