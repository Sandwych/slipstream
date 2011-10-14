using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;

namespace ObjectServer.Client.Model
{
    public class Entity : IEntity
    {
        public const long IdForCreation = -1;
        private readonly IDictionary<string, IField> fields = new Dictionary<string, IField>();

        internal Entity(IEntityCollection owner, long id)
        {
            if (id <= 0)
            {
                this.Id = IdForCreation;
            }
            else
            {
                this.Id = id;
            }

            this.Owner = owner;
        }

        #region IEntity Members

        public string ModelName { get { return this.Owner.ModelName; } }

        public long Id { get; private set; }

        public IField this[string fieldName]
        {
            get
            {
                //TODO 加载 Lazy 字段
                return this.fields[fieldName];
            }
            set
            {
                this.fields[fieldName] = value;
            }
        }

        #endregion

        public void Save(IRemoteService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

        #region IEntity Members

        public IEntityCollection Owner { get; private set; }

        public IEntity Parent
        {
            get
            {
                Debug.Assert(this.Owner != null);
                return this.Owner.Parent;
            }
        }

        public void SetFieldValues(IDictionary<string, object> record)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            foreach (var p in record)
            {
                var field = new Field(p.Key, p.Value);
                this.fields.Add(p.Key, field);
            }
        }

        #endregion
    }
}
