using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Exceptions;
using ObjectServer.Sql;

namespace ObjectServer.Model
{
    /// <summary>
    /// 单表继承的子类表基类
    /// </summary>
    public abstract class AbstractExtendedModel : AbstractResource, IModel
    {
        private readonly IFieldCollection fields;

        public AbstractExtendedModel(string name)
            : base(name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            this.fields = new FieldCollection(this);
        }

        public override void Load(IDBProfile db)
        {
            if (db == null)
            {
                throw new ArgumentNullException("db");
            }

            if (!db.ContainsResource(this.Name))
            {
                var msg = string.Format("Cannot found model '{0}'", this.Name);
                throw new ResourceNotFoundException(msg, this.Name);
            }

            var baseModel = db.GetResource(this.Name);
            baseModel.Load(db);
        }

        public ICollection<InheritanceInfo> Inheritances
        {
            get { throw new NotSupportedException(); }
        }

        public IFieldCollection Fields
        {
            get
            {
                Debug.Assert(this.fields != null);
                return this.fields;
            }
        }

        public IField[] GetAllStorableFields()
        {
            throw new NotSupportedException();
        }

        public override string[] GetReferencedObjects()
        {
            throw new NotSupportedException();
        }

        public bool AutoMigration
        {
            get { throw new NotSupportedException(); }
        }

        public string TableName
        {
            get { throw new NotSupportedException(); }
        }

        public bool Hierarchy
        {
            get { throw new NotSupportedException(); }
        }

        public bool CanCreate
        {
            get { throw new NotSupportedException(); }
        }

        public bool CanRead
        {
            get { throw new NotSupportedException(); }
        }

        public bool CanWrite
        {
            get { throw new NotSupportedException(); }
        }

        public bool CanDelete
        {
            get { throw new NotSupportedException(); }
        }

        public bool LogCreation
        {
            get { throw new NotSupportedException(); }
        }

        public bool LogWriting
        {
            get { throw new NotSupportedException(); }
        }

        public NameGetter NameGetter
        {
            get { throw new NotSupportedException(); }
        }

        public long CountInternal(
            IServiceScope ctx, object[] constraints = null)
        {
            throw new NotSupportedException();
        }

        public long[] SearchInternal(
            IServiceScope ctx, object[] constraints = null,
            OrderExpression[] orders = null, long offset = 0, long limit = 0)
        {
            throw new NotSupportedException();

        }

        public long CreateInternal(
            IServiceScope ctx, IDictionary<string, object> propertyBag)
        {
            throw new NotSupportedException();
        }

        public void WriteInternal(
            IServiceScope scope, long id, IDictionary<string, object> record)
        {
            throw new NotSupportedException();
        }

        public Dictionary<string, object>[] ReadInternal(
            IServiceScope scope, long[] ids, string[] fields = null)
        {
            throw new NotSupportedException();
        }

        public void DeleteInternal(IServiceScope scope, long[] ids)
        {
            throw new NotSupportedException();
        }

        public dynamic Browse(IServiceScope scope, long id)
        {
            throw new NotSupportedException();
        }

        public override bool DatabaseRequired
        {
            get { throw new NotSupportedException(); }
        }
    }
}
