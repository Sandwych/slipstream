using System;
using System.Net;
using System.Collections.Generic;

namespace ObjectServer.Client.Model
{
    public interface IEntityCollection : ICollection<IEntity>
    {
        string ModelName { get; }
        IEntity Parent { get; }

        void Load(IRemoteService service);
        void Load(IRemoteService service, object[] constraint);
        void Load(IRemoteService service, long[] ids);

        void Save(IRemoteService service);

        IEntity NewEntity();
    }
}
