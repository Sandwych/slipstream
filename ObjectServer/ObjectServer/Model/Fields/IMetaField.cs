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
        FieldGetter Getter { get; set; }
        FieldDefaultProc DefaultProc { get; set; }
        FieldType Type { get; }
        int Size { get; set; }
        bool IsRequired { get; }
        string Relation { get; set; }
        string OriginField { get; set; }
        string RelatedField { get; set; }
        bool Internal { get; }
        bool IsReadonly { get; set; }
        bool Lazy { get; set; }
        bool IsScalar { get; }
        OnDeleteAction OnDeleteAction { get; set; }

        IDictionary<string, string> Options { get; }

        /// <summary>
        /// 是否映射到实际的数据库列
        /// </summary>
        /// <returns></returns>
        bool IsColumn();

        void Validate();

        Dictionary<long, object> GetFieldValues(
            IResourceScope scope, List<Dictionary<string, object>> records);

        Dictionary<long, object> SetFieldValues(
            IResourceScope scope, IList<Dictionary<string, object>> records);

        object BrowseField(IResourceScope scope, IDictionary<string, object> record);

        #region Fluent interface 

        IMetaField SetLabel(string label);
        IMetaField Required();
        IMetaField NotRequired();
        IMetaField SetGetter(FieldGetter fieldGetter);
        IMetaField SetDefaultProc(FieldDefaultProc defaultProc);
        IMetaField SetSize(int size);
        IMetaField Readonly();
        IMetaField NotReadonly();
        IMetaField OnDelete(OnDeleteAction act);
     
        #endregion

    }
}
