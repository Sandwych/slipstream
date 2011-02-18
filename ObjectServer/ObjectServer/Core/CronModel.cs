using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [ServiceObject] 
    public class CronModel : ModelBase
    {

        public CronModel()
            : base()
        {
            this.Name = "core.cron";            
        }


    }
}
