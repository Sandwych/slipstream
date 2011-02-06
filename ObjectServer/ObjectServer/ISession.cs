using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace ObjectServer
{
    public interface ISession
    {
        IDbConnection Connection { get; }
    }
}
