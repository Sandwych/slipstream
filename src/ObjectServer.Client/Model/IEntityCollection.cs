using System;
using System.Net;
using System.Collections.Generic;

namespace ObjectServer.Client.Model
{
    public interface IEntityCollection : ICollection<IEntity>
    {
        string ModelName { get; }
        IEntity Parent { get; }

        void Load(IRootService service);
        void Load(IRootService service, object[] constraint);
        void Load(IRootService service, long[] ids);

        void Save(IRootService service);

        IEntity NewEntity();
    }
}
