using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Data;

namespace ObjectServer
{
    public interface IServiceContext : IDisposable, IEquatable<IServiceContext>
    {
        Session Session { get; }

        IResource GetResource(string resName);
        int GetResourceDependencyWeight(string resName);
        IDBContext DBContext { get; }
    }
}
