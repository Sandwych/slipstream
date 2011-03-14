using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [Resource]
    public sealed class WizardModel : AbstractTableModel
    {

        public WizardModel()
            : base("core.wizard")
        {
        }


    }
}
