using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [Resource]
    public sealed class ViewModel : TableModel
    {

        public ViewModel()
            : base("core.view")
        {
        }

    }
}
