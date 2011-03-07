using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IResourceContainer
    {
        void RegisterResource(IResource res);
        dynamic GetResource(string objName);
        dynamic this[string resName] { get; }

        void InitializeRegisteredObjects();
    }
}
