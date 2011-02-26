using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IObjectCollection
    {
        void Initialize();

        void RegisterObject(IServiceObject so);

        IServiceObject this[string objName] { get; }
    }
}
