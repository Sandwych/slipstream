using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public abstract class AbstractExtendedModel : AbstractModel
    {
        public AbstractExtendedModel(string name)
            : base(name)
        {

        }

        public override void Load(IDatabaseProfile db)
        {
            if (!db.ContainsResource(this.Name))
            {
                var msg = string.Format("Cannot found model '{0}'", this.Name);
                throw new ResourceNotFoundException(msg, this.Name);
            }

            base.Load(db);
        }

        public override string TableName
        {
            get { throw new NotSupportedException(); }
            protected set { throw new NotSupportedException(); }
        }

        public override bool Hierarchy
        {
            get { throw new NotSupportedException(); }
            protected set { throw new NotSupportedException(); }
        }

        public override bool CanCreate
        {
            get { throw new NotSupportedException(); }
            protected set { throw new NotSupportedException(); }
        }

        public override bool CanRead
        {
            get { throw new NotSupportedException(); }
            protected set { throw new NotSupportedException(); }
        }

        public override bool CanWrite
        {
            get { throw new NotSupportedException(); }
            protected set { throw new NotSupportedException(); }
        }

        public override bool CanDelete
        {
            get { throw new NotSupportedException(); }
            protected set { throw new NotSupportedException(); }
        }

        public override bool LogCreation
        {
            get { throw new NotSupportedException(); }
            protected set { throw new NotSupportedException(); }
        }

        public override bool LogWriting
        {
            get { throw new NotSupportedException(); }
            protected set { throw new NotSupportedException(); }
        }

        public override NameGetter NameGetter
        {
            get { throw new NotSupportedException(); }
            protected set { throw new NotSupportedException(); }
        }

        public override long[] SearchInternal(
            IResourceScope ctx, object[][] domain = null, long offset = 0, long limit = 0)
        {
            throw new NotSupportedException();

        }

        public override long CreateInternal(
            IResourceScope ctx, IDictionary<string, object> propertyBag)
        {
            throw new NotSupportedException();
        }

        public override void WriteInternal(
            IResourceScope ctx, long id, IDictionary<string, object> record)
        {
            throw new NotSupportedException();
        }

        public override Dictionary<string, object>[] ReadInternal(
            IResourceScope ctx, IEnumerable<long> ids, IEnumerable<string> fields = null)
        {
            throw new NotSupportedException();
        }

        public override void DeleteInternal(IResourceScope ctx, IEnumerable<long> ids)
        {
            throw new NotSupportedException();
        }

        public override dynamic Browse(IResourceScope ctx, long id)
        {
            throw new NotSupportedException();
        }

        public override bool DatabaseRequired
        {
            get { throw new NotSupportedException(); }
        }
    }
}
