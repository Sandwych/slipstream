using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class ViewModel : ModelBase
    {

        public ViewModel()
        {
            this.Name = "view.action";
        }

    }
}
