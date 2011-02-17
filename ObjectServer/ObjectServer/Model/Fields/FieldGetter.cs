using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model.Fields
{
    public delegate Dictionary<long, object>
        FieldGetter(ISession session, IEnumerable<long> ids);
}
