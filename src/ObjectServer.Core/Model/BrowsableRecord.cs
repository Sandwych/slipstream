using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Diagnostics;

namespace ObjectServer.Model
{
    //TODO 处理 lazy 的字段
    public sealed class BrowsableRecord : DynamicObject
    {
        private IDictionary<string, object> record;
        private IModel metaModel;
        private ITransactionContext scope;

        public BrowsableRecord(ITransactionContext scope, IModel metaModel, long id)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            if (metaModel == null)
            {
                throw new ArgumentNullException("metaModel");
            }

            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException("id");
            }

            this.metaModel = metaModel;
            this.scope = scope;
            this.record = metaModel.ReadInternal(scope, new long[] { id }, null)[0];
        }

        public BrowsableRecord(ITransactionContext scope,
            IModel metaModel, IDictionary<string, object> record)
        {
            if (scope == null)
            {
                throw new ArgumentNullException("scope");
            }

            if (metaModel == null)
            {
                throw new ArgumentNullException("metaModel");
            }

            if (record == null)
            {
                throw new ArgumentNullException("record");
            }

            this.metaModel = metaModel;
            this.scope = scope;
            this.record = record;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            throw new NotSupportedException();
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            throw new NotSupportedException();
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Debug.Assert(this.record != null);
            Debug.Assert(this.metaModel != null);
            Debug.Assert(this.scope != null);

            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }
            return this.GetPropertyValue(binder.Name, out result);
        }

        private bool GetPropertyValue(string memberName, out object result)
        {
            Debug.Assert(!string.IsNullOrEmpty(memberName));

            result = null;
            if (!metaModel.Fields.ContainsKey(memberName))
            {
                return false;
            }

            var metaField = metaModel.Fields[memberName];
            result = metaField.BrowseField(this.scope, this.record);
            return true;
        }

    }
}
