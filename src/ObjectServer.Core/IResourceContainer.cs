using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using ObjectServer.Data;

namespace ObjectServer
{
    public interface IResourceContainer
    {
        void RegisterResource(IResource res);
        IResource GetResource(string resName);
        bool ContainsResource(string resName);
        int GetResourceDependencyWeight(string resName);
        IResource[] GetAllResources();

        void InitializeAllResources(ITransactionContext tc, bool update);
    }
}
