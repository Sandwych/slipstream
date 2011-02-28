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

        void RegisterObject(IObjectService so);

        IObjectService this[string objName] { get; }
    }
}
