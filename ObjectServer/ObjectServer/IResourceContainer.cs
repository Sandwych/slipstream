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

        void RegisterObject(IResource so);

        IResource Resolve(string objName);
    }
}
