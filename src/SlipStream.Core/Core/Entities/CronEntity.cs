using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlipStream.Entity;

namespace SlipStream.Core
{
    [Resource]
    public sealed class CronEntity : AbstractSqlEntity
    {

        public CronEntity()
            : base("core.cron")
        {
        }

    }
}
