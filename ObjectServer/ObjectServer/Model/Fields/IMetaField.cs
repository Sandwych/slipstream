using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IMetaField : IFieldDescriptor
    {
        FieldValueGetter Getter { get; set; }
        FieldDefaultValueGetter DefaultProc { get; set; }

        void Validate();

        Dictionary<long, object> GetFieldValues(
            IResourceScope scope, ICollection<Dictionary<string, object>> records);

        object SetFieldValue(IResourceScope scope, object value);

        object BrowseField(IResourceScope scope, IDictionary<string, object> record);

        #region Fluent interface

        IMetaField SetLabel(string label);
        IMetaField Required();
        IMetaField NotRequired();
        IMetaField ValueGetter(FieldValueGetter fieldGetter);
        IMetaField SetDefaultValueGetter(FieldDefaultValueGetter defaultGetter);
        IMetaField SetSize(int size);
        IMetaField SetHelp(string help);
        IMetaField Readonly();
        IMetaField NotReadonly();
        IMetaField OnDelete(OnDeleteAction act);
        IMetaField AsProperty();
        IMetaField SetOptions(IDictionary<string, string> options);

        #endregion

    }
}
