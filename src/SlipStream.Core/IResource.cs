using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;

using SlipStream.Data;

namespace SlipStream
{
    public interface IResource
    {
        string Label { get; }
        string Name { get; }
        string Module { get; }
        IDbDomain DbDomain { get; set; }
        ICollection<IService> Services { get; }

        /// <summary>
        /// 此对象引用（依赖）的其它对象名称
        /// </summary>
        string[] GetReferencedObjects();
        IService GetService(string name);

        /// <summary>
        /// 初始化资源
        /// </summary>
        /// <param name="update">是否进行涉及数据库等的更新动作</param>
        void Initialize(bool update);

        //从另一个资源合并字段与业务方法
        void MergeFrom(IResource res);
    }
}
