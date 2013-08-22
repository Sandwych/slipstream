using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Core
{
    [Resource]
    public sealed class WizardActionModel : AbstractSqlModel
    {

        public WizardActionModel()
            : base("core.action_wizard")
        {
            Inherit("core.action", "action");
            Fields.ManyToOne("action", "core.action")
                .SetLabel("Base Action").Required().OnDelete(OnDeleteAction.Cascade);
        }


    }
}
