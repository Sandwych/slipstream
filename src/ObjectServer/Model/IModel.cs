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

        long[] SearchInternal(IServiceScope ctx, object[][] domain = null, OrderExpression[] orders = null, long offset = 0, long limit = 0);
        long CreateInternal(IServiceScope ctx, IDictionary<string, object> propertyBag);
        void WriteInternal(IServiceScope ctx, long id, IDictionary<string, object> record);
        Dictionary<string, object>[] ReadInternal(
            IServiceScope ctx, long[] ids, string[] requiredFields = null);
        void DeleteInternal(IServiceScope ctx, long[] ids);

        dynamic Browse(IServiceScope ctx, long id);
    }
}
