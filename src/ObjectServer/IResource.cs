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
        string Label { get; }
        string Name { get; }
        string Module { get; }
        bool DatabaseRequired { get; }
        ICollection<IService> Services { get; }

        /// <summary>
        /// 此对象引用（依赖）的其它对象名称
        /// </summary>
        string[] GetReferencedObjects();
        IService GetService(string name);

        void Load(IDBProfile db);

        //从另一个资源合并字段与业务方法
        void MergeFrom(IResource res);
    }
}
