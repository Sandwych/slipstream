using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace ObjectServer.Model
{
    public interface IMetaModel : IResource, IModelDescriptor
    {
        IMetaField[] GetAllStorableFields();

        long[] SearchInternal(IServiceScope ctx, object[][] domain = null, OrderInfo[] orders = null, long offset = 0, long limit = 0);
        long CreateInternal(IServiceScope ctx, IDictionary<string, object> propertyBag);
        void WriteInternal(IServiceScope ctx, long id, IDictionary<string, object> record);
        Dictionary<string, object>[] ReadInternal(
            IServiceScope ctx, IEnumerable<long> ids, IEnumerable<string> requiredFields = null);
        void DeleteInternal(IServiceScope ctx, IEnumerable<long> ids);

        dynamic Browse(IServiceScope ctx, long id);
    }
}
