using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;

using ObjectServer.Backend;

namespace ObjectServer
{
    public interface IResource
    {
        void Load(IDatabase db);

        string Label { get; }
        string Name { get; }
        string Module { get; }

        MethodInfo GetServiceMethod(string name);
        ICollection<MethodInfo> ServiceMethods { get; }

        bool DatabaseRequired { get; }

        /// <summary>
        /// 此对象引用（依赖）的其它对象名称
        /// </summary>
        string[] GetReferencedObjects();

        //从另一个资源合并字段与业务方法
        void MergeFrom(IResource res);

    }
}
