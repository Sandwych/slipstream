using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Data;

namespace ObjectServer
{
    public interface IDBProfile : IDisposable, IResourceContainer
    {
        IDBContext DBContext { get; }
    }
}
