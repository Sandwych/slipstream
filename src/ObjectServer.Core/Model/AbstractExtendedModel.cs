using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using ObjectServer.Data;
using ObjectServer.Exceptions;

namespace ObjectServer.Model
{
    /// <summary>
    /// 单表继承的子类表基类
    /// </summary>
    public abstract class AbstractExtendedModel : AbstractResource, IModel
    {
        private readonly IFieldCollection fields;

        protected AbstractExtendedModel(string name)
            : base(name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            this.fields = new FieldCollection(this);
        }

        public override void Initialize(IServiceContext tc, bool update)
        {
            if (tc == null)
            {
                throw new ArgumentNullException("ctx");
            }

            var baseModel = tc.GetResource(Name);

            baseModel.Initialize(tc, update);

        }

        public ICollection<InheritanceDescriptor> Inheritances
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

        public Dictionary<string, object> GetFieldDefaultValuesInternal(
            IServiceContext ctx, string[] fields)
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

        public bool IsVersioned
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
            IServiceContext ctx, object[] constraint)
        {
            throw new NotSupportedException();
        }

        public long[] SearchInternal(
            IServiceContext ctx, object[] constraints,
            OrderExpression[] orders, long offset, long limit)
        {
            throw new NotSupportedException();

        }

        public long CreateInternal(
            IServiceContext ctx, IDictionary<string, object> propertyBag)
        {
            throw new NotSupportedException();
        }

        public void WriteInternal(
            IServiceContext scope, long id, IDictionary<string, object> record)
        {
            throw new NotSupportedException();
        }

        public Dictionary<string, object>[] ReadInternal(
            IServiceContext scope, long[] ids, string[] requiredFields)
        {
            throw new NotSupportedException();
        }

        public void DeleteInternal(IServiceContext scope, long[] ids)
        {
            throw new NotSupportedException();
        }

        public dynamic Browse(IServiceContext scope, long id)
        {
            throw new NotSupportedException();
        }

        public dynamic BrowseMany(IServiceContext scope, long[] ids)
        {
            throw new NotSupportedException();
        }

        public void ImportRecord(
              IServiceContext ctx, bool noUpdate, IDictionary<string, object> record, string key)
        {
            throw new NotSupportedException();
        }

        public Dictionary<string, object>[] GetFieldsInternal(IServiceContext ctx, string[] fields)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<OrderExpression> Order
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public IModelDescriptor OrderBy(IEnumerable<OrderExpression> order)
        {
            throw new NotSupportedException();
        }
    }
}
