using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Model;

namespace SlipStream.Core
{
    [Resource]
    public sealed class CronModel : AbstractSqlModel
    {

        public CronModel()
            : base("core.cron")
        {
        }

    }
}
