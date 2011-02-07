using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class View : Model.ModelBase
    {

        public View()
        {
            this.Name = "View.Action";
        }

    }
}
