using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Core
{
    [Resource]
    public sealed class WizardActionEntity : AbstractSqlEntity
    {

        public WizardActionEntity() : base("core.action_wizard")
        {
            Inherit("core.action", "action");
            Fields.ManyToOne("action", "core.action")
                .WithLabel("Base Action").WithRequired().OnDelete(OnDeleteAction.Cascade);
        }


    }
}
