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

        long CountInternal(IServiceContext ctx, object[] constraints = null);
        long[] SearchInternal(IServiceContext ctx, object[] constraints = null, OrderExpression[] orders = null, long offset = 0, long limit = 0);
        long CreateInternal(IServiceContext ctx, IDictionary<string, object> propertyBag);
        void WriteInternal(IServiceContext ctx, long id, IDictionary<string, object> record);
        Dictionary<string, object>[] ReadInternal(IServiceContext ctx, long[] ids, string[] requiredFields = null);
        void DeleteInternal(IServiceContext ctx, long[] ids);
        dynamic Browse(IServiceContext ctx, long id);
    }
}
