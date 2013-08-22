using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using SlipStream.Data;

namespace SlipStream
{
    public interface IResourceContainer
    {
        bool RegisterResource(IResource res);
        IResource GetResource(string resName);
        bool ContainsResource(string resName);
        int GetResourceDependencyWeight(string resName);
        IResource[] GetAllResources();

    }
}
