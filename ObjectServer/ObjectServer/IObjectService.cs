using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IObjectService
    {
        void Initialize(IDatabase db);

        string Label { get; }
        string Name { get; }
        string Module { get; }

        MethodInfo GetServiceMethod(string name);

        bool DatabaseRequired { get; }

        IServiceContainer Pool { get; }

        /// <summary>
        /// 此对象引用（依赖）的其它对象名称
        /// </summary>
        string[] GetReferencedObjects();
    }
}
