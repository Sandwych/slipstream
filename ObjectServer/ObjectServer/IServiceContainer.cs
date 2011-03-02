using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IServiceContainer
    {
        void Initialize();

        void RegisterObject(IObjectService so);

        IObjectService Resolve(string objName);
    }
}
