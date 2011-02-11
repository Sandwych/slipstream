using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class Module : ModelBase
    {

        public Module()
        {
            this.Automatic = false;
            this.Name = "Core.Module";
        }

    }
}
