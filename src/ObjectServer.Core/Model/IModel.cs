using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

using ObjectServer.Sql;

namespace ObjectServer.Model
{
    public interface IModel : IResource, IModelDescriptor
    {
        IField[] GetAllStorableFields();

        long CountInternal(ITransactionContext ctx, object[] constraints = null);
        long[] SearchInternal(ITransactionContext ctx, object[] constraints = null, OrderExpression[] orders = null, long offset = 0, long limit = 0);
        long CreateInternal(ITransactionContext ctx, IDictionary<string, object> propertyBag);
        void WriteInternal(ITransactionContext ctx, long id, IDictionary<string, object> record);
        Dictionary<string, object>[] ReadInternal(ITransactionContext ctx, long[] ids, string[] requiredFields = null);
        void DeleteInternal(ITransactionContext ctx, long[] ids);
        dynamic Browse(ITransactionContext ctx, long id);
    }
}
