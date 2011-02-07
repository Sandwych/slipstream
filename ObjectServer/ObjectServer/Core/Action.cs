using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class Action : Model.ModelBase
    {

        public Action()
        {
            this.Name = "Core.Action";
        }

    }
}
