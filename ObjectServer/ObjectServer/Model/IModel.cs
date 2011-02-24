using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public interface IModel : IServiceObject
    {
        IDictionary<string, IMetaField> DefinedFields { get; }

        string TableName { get; }
        bool Hierarchy { get; }
        bool CanCreate { get; }
        bool CanRead { get; }
        bool CanWrite { get; }
        bool CanDelete { get; }

        NameGetter NameGetter { get; }

        object[] Search(ICallingContext callingContext, object[][] domain, long offset, long limit);
        long Create(ICallingContext callingContext, IDictionary<string, object> propertyBag);
        void Write(ICallingContext callingContext, object id, IDictionary<string, object> record);
        Dictionary<string, object>[] Read(ICallingContext callingContext, object[] ids, object[] fields);
        void Delete(ICallingContext callingContext, object[] ids);
    }
}
