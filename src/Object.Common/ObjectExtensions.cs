using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object self)
        {
            return (self == null) || (self is DBNull) || (self == DBNull.Value);
        }
    }
}
