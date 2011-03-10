using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IResourceScope : IDisposable, IEquatable<IResourceScope>
    {
        IDatabase Database { get; }

        Session Session { get; }

    }
}
