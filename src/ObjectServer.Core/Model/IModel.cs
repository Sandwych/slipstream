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
        Dictionary<string, object> GetFieldDefaultValuesInternal(IServiceContext ctx, string[] fields);
        long CountInternal(IServiceContext ctx, object[] constraint);
        long[] SearchInternal(IServiceContext ctx, object[] constraint, OrderExpression[] orders, long offset, long limit);
        long CreateInternal(IServiceContext ctx, IDictionary<string, object> propertyBag);
        void WriteInternal(IServiceContext ctx, long id, IDictionary<string, object> record);
        Dictionary<string, object>[] ReadInternal(IServiceContext ctx, long[] ids, string[] requiredFields);
        void DeleteInternal(IServiceContext ctx, long[] ids);
        Dictionary<string, object>[] GetFieldsInternal(IServiceContext ctx, string[] fields);
        dynamic Browse(IServiceContext ctx, long id);
        dynamic BrowseMany(IServiceContext ctx, long[] ids);
        void ImportRecord(IServiceContext ctx, bool noUpdate, IDictionary<string, object> record, string key);
    }
}
