using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectServer.Client.Model
{
    public class EntitySet : IEntityCollection
    {
        private readonly List<IEntity> entities = new List<IEntity>();
        private readonly IDictionary<string, MetaField> metaFields = new Dictionary<string, MetaField>();

        public EntitySet(string modelName, IEnumerable<MetaField> metaFields, IEntity parent = null)
        {
            this.ModelName = modelName;
            this.Parent = parent;

            this.metaFields = metaFields.ToDictionary(i => i.Name);
        }

        public string ModelName { get; private set; }

        public IEntity Parent { get; private set; }

        #region ICollection<IEntity> Members

        public void Add(IEntity item)
        {
            this.entities.Add(item);
        }

        public void Clear()
        {
            this.entities.Clear();
        }

        public bool Contains(IEntity item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(IEntity[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { return this.entities.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(IEntity item)
        {
            throw new NotImplementedException();
        }

        public void RemoveById(long id)
        {
            int i = 0;
            for (i = 0; i < this.entities.Count; i++)
            {
                if (this.entities[i].Id == id)
                {
                    break;
                }
            }

            if (i < this.entities.Count)
            {
                this.entities.RemoveAt(i);
            }
        }

        #endregion

        #region IEnumerable<IEntity> Members

        public System.Collections.Generic.IEnumerator<IEntity> GetEnumerator()
        {
            return this.entities.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.entities.GetEnumerator();
        }

        #endregion

        public void Load(IRemoteService service)
        {
        }

        public void Load(IRemoteService service, long[] ids)
        {
            var fields = this.metaFields.Keys.ToArray();
            var args = new object[] { ids, fields };
            service.Execute(this.ModelName, "Read", args, (result, error) =>
            {
                var records = ((object[])result).Cast<IDictionary<string, object>>();
                foreach (var r in records)
                {
                    var entity = this.NewEntity();
                    entity.SetFieldValues(r);
                    this.entities.Add(entity);
                }
            });
        }

        public void Load(IRemoteService service, object[] constraint)
        {
        }

        public void Save(IRemoteService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException("serive");
            }

            foreach (var e in this.entities)
            {
                e.Save(service);
            }
        }

        public IEntity NewEntity()
        {
            return new Entity(this, -1);
        }
    }
}
