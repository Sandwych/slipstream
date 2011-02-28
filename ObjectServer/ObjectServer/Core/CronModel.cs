using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model;

namespace ObjectServer.Core
{
    [ServiceObject]
    public sealed class CronModel : TableModel
    {

        public CronModel()
            : base("core.cron")
        {
        }


    }
}
