using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace ObjectServer.Model
{
    internal interface IModel : IResource
    {
        IMetaFieldCollection Fields { get; }

        string TableName { get; }
        bool Hierarchy { get; }
        bool CanCreate { get; }
        bool CanRead { get; }
        bool CanWrite { get; }
        bool CanDelete { get; }

        bool LogCreation { get; }
        bool LogWriting { get; }

        NameGetter NameGetter { get; }

        object[] SearchInternal(IContext ctx, object[] domain = null, long offset = 0, long limit = 0);
        long CreateInternal(IContext ctx, IDictionary<string, object> propertyBag);
        void WriteInternal(IContext ctx, object id, IDictionary<string, object> record);
        Dictionary<string, object>[] ReadInternal(IContext ctx, object[] ids, object[] fields = null);
        void DeleteInternal(IContext ctx, object[] ids);

        dynamic Browse(IContext ctx, object id);
    }
}
