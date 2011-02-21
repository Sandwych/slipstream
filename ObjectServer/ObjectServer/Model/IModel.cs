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

        object[] Search(ISession session, object[][] domain, long offset, long limit);
        long Create(ISession session, IDictionary<string, object> propertyBag);
        void Write(ISession session, object id, IDictionary<string, object> record);
        Dictionary<string, object>[] Read(ISession session, object[] ids, object[] fields);
        void Delete(ISession session, object[] ids);
    }
}
