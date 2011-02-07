using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Core
{

    [ServiceObject]
    public sealed class Group : Model.ModelBase
    {

        public Group()
        {
            this.Name = "Core.Group";
        }

    }
}
