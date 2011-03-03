using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace ObjectServer.Model
{
    public interface IModel : IResource
    {
        IMetaFieldCollection Fields { get; }

        string TableName { get; }
        bool Hierarchy { get; }
        bool CanCreate { get; }
        bool CanRead { get; }
        bool CanWrite { get; }
        bool CanDelete { get; }

        NameGetter NameGetter { get; }

        object[] Search(IContext callingContext, object[] domain, long offset, long limit);
        long Create(IContext callingContext, IDictionary<string, object> propertyBag);
        void Write(IContext callingContext, object id, IDictionary<string, object> record);
        Dictionary<string, object>[] Read(IContext callingContext, object[] ids, object[] fields);
        void Delete(IContext callingContext, object[] ids);

        dynamic Browse(IContext callingContext, object id);
    }
}
