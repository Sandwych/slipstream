using System;
using System.Net;
using System.Collections.Generic;

namespace ObjectServer.Client.Model
{
    public interface IEntityCollection : ICollection<IEntity>
    {
        string ModelName { get; }
        IEntity Parent { get; }

        void Save(IRemoteService service);
    }
}
