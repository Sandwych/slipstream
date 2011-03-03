using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IDatabase : IDisposable, IGlobalObject
    {
        IResourceContainer ServiceObjects { get; }
        IDataContext DataContext { get; }
    }
}
