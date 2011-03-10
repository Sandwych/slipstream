using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public delegate Dictionary<long, object>
        FieldGetter(IResourceScope session, object[] ids);
}
