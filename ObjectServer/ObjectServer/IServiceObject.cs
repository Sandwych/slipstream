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
        void Initialize(IDatabaseContext db, ObjectPool pool);

        string Label { get; }
        string Name { get; }

        MethodInfo GetServiceMethod(string name);

        bool DatabaseRequired { get; }

        ObjectPool Pool { get; }

        /// <summary>
        /// 此对象引用（依赖）的其它对象名称
        /// </summary>
        string[] ReferencedObjects { get; }

    }
}
