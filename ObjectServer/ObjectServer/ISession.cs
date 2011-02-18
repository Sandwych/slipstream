using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface ISession : IDisposable
    {
        ObjectPool Pool { get; }
        IDatabase Database { get; }
    }
}
