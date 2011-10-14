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

        public Entity(string modelName, long id, IEntityCollection owner)
        {
            if (string.IsNullOrEmpty(modelName))
            {
                throw new ArgumentNullException("modelName");
            }

            if (id <= 0)
            {
                this.Id = IdForCreation;
            }
            else
            {
                this.Id = id;
            }

            this.Owner = owner;

            Debug.Assert(!(this.Owner != null && modelName != this.Owner.ModelName));
        }

        #region IEntity Members

        public string ModelName { get; private set; }

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

        public void Load(IRemoteService service)
        {
            Debug.Assert(this.Id > 0);
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            //获取字段元信息
            var args = new object[] { };
            service.BeginExecute(this.ModelName, "GetFields", args, (result, error) =>
            {
                //读取字段值
                var metaFields = ((object[])result).Select(r => (IDictionary<string, object>)r);
                var fieldNames = metaFields.Select(mf => (string)mf["name"]);
                args = LoadData(service, fieldNames);
            });
        }

        private object[] LoadData(IRemoteService service, IEnumerable<string> fieldNames)
        {
            object[] args;
            var ids = new long[] { this.Id };
            args = new object[] { ids, fieldNames };
            service.BeginExecute(this.ModelName, "Read", args, (result1, error1) =>
            {
                var objs = (object[])result1;
                var records = objs.Select(r => (Dictionary<string, object>)r);
                var record = records.First();
                foreach (var p in record)
                {
                    this.fields[p.Key] = new Field(p.Key, p.Value);
                }
            });
            return args;
        }

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

        #endregion
    }
}
