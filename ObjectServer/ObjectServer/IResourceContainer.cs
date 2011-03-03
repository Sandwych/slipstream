using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IResourceContainer
    {
        void Initialize();

        void RegisterResource(IResource res);

        IResource Resolve(string objName);
        IResource this[string resName] { get; }
    }
}
