using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IDBProfile : IDisposable, IResourceContainer
    {
        IDBConnection Connection { get; }
    }
}
