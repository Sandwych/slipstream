using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SlipStream.Entity
{
    [CLSCompliant(true)]
    public delegate Criterion[] CriterionConverter(IServiceContext ctx, Criterion criterion);
}
