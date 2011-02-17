using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Model.Fields;

namespace ObjectServer.Backend
{
    public interface ISqlTypeConverter
    {
        string ToSqlType(IField field);
    }
}
