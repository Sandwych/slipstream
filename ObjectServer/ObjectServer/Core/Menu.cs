using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class Menu : Model.ModelBase
    {

        public Menu()
        {
            this.Name = "Core.Menu";
        }

    }
}
