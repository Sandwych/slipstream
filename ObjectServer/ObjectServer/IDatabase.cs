using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IDatabase : IDisposable, IGlobalObject
    {
        IServiceContainer ServiceObjects { get; }
        IDataContext DataContext { get; }
    }
}
