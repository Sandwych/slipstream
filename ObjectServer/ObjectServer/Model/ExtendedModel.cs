using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectServer.Model
{
    public class ExtendedModel : ModelBase
    {
        public ExtendedModel(string name)
            : base(name)
        {
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

        public override object[] SearchInternal(
            IContext ctx, object[] domain = null, long offset = 0, long limit = 0)
        {
            throw new NotSupportedException();

        }

        public override long CreateInternal(
            IContext ctx, IDictionary<string, object> propertyBag)
        {
            throw new NotSupportedException();
        }

        public override void WriteInternal(
            IContext ctx, long id, IDictionary<string, object> record)
        {
            throw new NotSupportedException();
        }

        public override Dictionary<string, object>[] ReadInternal(
            IContext ctx, object[] ids, IEnumerable<string> fields = null)
        {
            throw new NotSupportedException();
        }

        public override void DeleteInternal(IContext ctx, IEnumerable<long> ids)
        {
            throw new NotSupportedException();
        }

        public override dynamic Browse(IContext ctx, long id)
        {
            throw new NotSupportedException();
        }

        public override bool DatabaseRequired
        {
            get { throw new NotSupportedException(); }
        }
    }
}
