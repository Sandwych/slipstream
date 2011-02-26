using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IObjectPool : IDisposable
    {
        string DatabaseName { get; }

        IDataContext DatabaseContext { get; }

        void AddServiceObject(IServiceObject obj);

        IServiceObject this[string objName] { get; }

        bool Contains(string objName);
    }
}
