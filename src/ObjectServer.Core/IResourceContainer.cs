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

        /// <summary>
        /// 初始化容器里的所有资源，此方法允许多次调用，但此方法仅会调用一次资源的 Initialize()
        /// </summary>
        void InitializeAllResources(IDBContext conn, bool update);

    }
}
