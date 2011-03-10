using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IDatabaseProfile : IDisposable, IResourceContainer
    {
        IDataContext DataContext { get; }
        
    }
}
