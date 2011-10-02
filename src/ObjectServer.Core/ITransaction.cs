using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer
{
    public interface ITransaction
    {
        IResource Resource { get; }
        string Name { get; }
        string Help { get; }

        object Invoke(IResource self, IServiceContext scope, params object[] args);
    }
}
