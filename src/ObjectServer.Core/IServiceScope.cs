using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Data;

namespace ObjectServer
{
    public interface IServiceScope : IDisposable, IEquatable<IServiceScope>
    {
        Session Session { get; }

        IResource GetResource(string resName);
        IDBContext DBContext { get; }
    }
}
