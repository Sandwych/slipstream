using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Entity
{
    public delegate IDictionary<long, string>
        NameGetter(IServiceContext ctx, long[] ids);
}
