﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Core
{

    /// <summary>
    /// WindowAction 与 View 的关联表
    /// </summary>
    [Resource]
    public sealed class WindowActionViewEntity : AbstractSqlEntity
    {

        public WindowActionViewEntity() : base("core.action_window_view")
        {
            Fields.Integer("ordinal").WithRequired().WithLabel("Ordinal");
            Fields.ManyToOne("view", "core.view").WithLabel("Related View").WithRequired();
            Fields.ManyToOne("window_action", "core.action_window")
                .WithLabel("Related Window Action").WithRequired();
        }

    }
}
