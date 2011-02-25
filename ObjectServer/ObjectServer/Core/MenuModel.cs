using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class MenuModel : TableModel
    {

        public MenuModel()
            : base("core.menu")
        {

        }

    }
}
