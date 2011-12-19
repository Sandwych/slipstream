using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace ObjectServer.Model
{
    public interface IModel : IResource, IModelDescriptor
    {
        IField[] GetAllStorableFields();
        Dictionary<string, object> GetFieldDefaultValuesInternal(string[] fields);
        long CountInternal(object[] constraint);
        long[] SearchInternal(object[] constraint, OrderExpression[] orders, long offset, long limit);
        long CreateInternal(IDictionary<string, object> propertyBag);
        void WriteInternal(long id, IDictionary<string, object> record);
        Dictionary<string, object>[] ReadInternal(long[] ids, string[] requiredFields);
        void DeleteInternal(long[] ids);
        Dictionary<string, object>[] GetFieldsInternal(string[] fields);
        dynamic Browse(long id);
        dynamic BrowseMany(long[] ids);
        void ImportRecord(bool noUpdate, IDictionary<string, object> record, string key);
    }
}
