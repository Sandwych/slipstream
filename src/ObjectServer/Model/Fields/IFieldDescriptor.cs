using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IFieldDescriptor
    {

        IMetaModel Model { get; }
        string Name { get; }
        string Label { get; }
        bool IsFunctional { get; }
        FieldType Type { get; }
        int Size { get; set; }
        bool IsRequired { get; }
        string Relation { get; set; }
        string OriginField { get; set; }
        string RelatedField { get; set; }
        bool Internal { get; }
        bool IsReadonly { get; set; }
        bool IsProperty { get; set; }
        bool Lazy { get; set; }
        bool IsScalar { get; }
        string Help { get; }
        OnDeleteAction OnDeleteAction { get; set; }

        IDictionary<string, string> Options { get; }

        /// <summary>
        /// 是否映射到实际的数据库列
        /// </summary>
        /// <returns></returns>
        bool IsColumn();
    }
}
