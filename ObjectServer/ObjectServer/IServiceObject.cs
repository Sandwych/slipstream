using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IServiceObject
    {
        void Initialize(IDatabaseContext db, IObjectPool pool);

        string Label { get; }
        string Name { get; }
        string Module { get; }

        MethodInfo GetServiceMethod(string name);

        bool DatabaseRequired { get; }

        IObjectPool Pool { get; }

        /// <summary>
        /// 此对象引用（依赖）的其它对象名称
        /// </summary>
        string[] GetReferencedObjects();

    }
}
