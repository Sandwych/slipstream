using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [ServiceObject]
    public class SessionModel : TableModel
    {

        public SessionModel()
            : base()
        {
            this.Name = "core.session";
        }


    }
}
