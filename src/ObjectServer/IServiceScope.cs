using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IServiceScope : IDisposable, IEquatable<IServiceScope>
    {
        IDatabaseProfile DatabaseProfile { get; }

        Session Session { get; }

        IResource GetResource(string resName);

    }
}
