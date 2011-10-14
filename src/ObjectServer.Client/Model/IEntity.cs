using System;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;

namespace ObjectServer.Client.Model
{
    public interface IEntity : INotifyPropertyChanged
    {
        IEntityCollection Owner { get; }
        IEntity Parent { get; }
        string ModelName { get; }
        long Id { get; }

        IField this[string fieldName] { get; set; }

        void Load(IRemoteService service);

        void Save(IRemoteService service);
    }
}
