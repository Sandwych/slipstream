using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface ICallingContext : IDisposable
    {
        IObjectPool Pool { get; } 
        IDatabaseContext Database { get; }
    }
}
