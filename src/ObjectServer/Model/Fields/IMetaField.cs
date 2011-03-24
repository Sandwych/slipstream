using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IMetaField
    {
        IMetaModel Model { get; }
        string Name { get; }
        string Label { get; }
        bool IsFunctional { get; }
        FieldValueGetter Getter { get; set; }
        FieldDefaultValueGetter DefaultProc { get; set; }
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

        void Validate();

        Dictionary<long, object> GetFieldValues(
            IServiceScope scope, ICollection<Dictionary<string, object>> records);

        object SetFieldValue(IServiceScope scope, object value);

        object BrowseField(IServiceScope scope, IDictionary<string, object> record);

        #region Fluent interface

        IMetaField SetLabel(string label);
        IMetaField Required();
        IMetaField NotRequired();
        IMetaField ValueGetter(FieldValueGetter fieldGetter);
        IMetaField DefaultValueGetter(FieldDefaultValueGetter defaultProc);
        IMetaField SetSize(int size);
        IMetaField SetHelp(string help);
        IMetaField Readonly();
        IMetaField NotReadonly();
        IMetaField OnDelete(OnDeleteAction act);
        IMetaField BeProperty();
        IMetaField SetOptions(IDictionary<string, string> options);

        #endregion

    }
}
