using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace ObjectServer.Model
{
    public interface IMetaModel : IResource, IModelDescriptor
    {
        string TableName { get; }
        NameGetter NameGetter { get; }
        IMetaField[] GetAllStorableFields();

        long[] SearchInternal(IResourceScope ctx, object[] domain = null, long offset = 0, long limit = 0);
        long CreateInternal(IResourceScope ctx, IDictionary<string, object> propertyBag);
        void WriteInternal(IResourceScope ctx, long id, IDictionary<string, object> record);
        Dictionary<string, object>[] ReadInternal(
            IResourceScope ctx, IEnumerable<long> ids, IEnumerable<string> requiredFields = null);
        void DeleteInternal(IResourceScope ctx, IEnumerable<long> ids);

        dynamic Browse(IResourceScope ctx, long id);
    }
}
