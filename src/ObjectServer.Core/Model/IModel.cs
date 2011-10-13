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

        long CountInternal(ITransactionContext ctx, object[] constraint);
        long[] SearchInternal(ITransactionContext ctx, object[] constraint, OrderExpression[] orders, long offset, long limit);
        long CreateInternal(ITransactionContext ctx, IDictionary<string, object> propertyBag);
        void WriteInternal(ITransactionContext ctx, long id, IDictionary<string, object> record);
        Dictionary<string, object>[] ReadInternal(ITransactionContext ctx, long[] ids, string[] requiredFields);
        void DeleteInternal(ITransactionContext ctx, long[] ids);
        Dictionary<string, object>[] GetFieldsInternal(ITransactionContext ctx);
        dynamic Browse(ITransactionContext ctx, long id);
    }
}
