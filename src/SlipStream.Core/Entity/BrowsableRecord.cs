using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Diagnostics;

namespace SlipStream.Entity
{
    //TODO 处理 lazy 的字段
    public sealed class BrowsableRecord : DynamicObject
    {
        private IDictionary<string, object> _record;
        private IEntity _metaEnity;

        public BrowsableRecord(IEntity metaModel, long id)
        {
            if (metaModel == null)
            {
                throw new ArgumentNullException("metaModel");
            }

            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException("id");
            }

            this._metaEnity = metaModel;
            this._record = metaModel.ReadInternal(new long[] { id }, null)[0];
        }

        public BrowsableRecord(IEntity metaModel, IDictionary<string, object> record)
        {
            if (metaModel == null)
            {
                throw new ArgumentNullException(nameof(metaModel));
            }

            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            this._metaEnity = metaModel;
            this._record = record;
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
            Debug.Assert(this._record != null);
            Debug.Assert(this._metaEnity != null);

            if (binder == null)
            {
                throw new ArgumentNullException(nameof(binder));
            }
            return this.GetPropertyValue(binder.Name, out result);
        }

        private bool GetPropertyValue(string memberName, out object result)
        {
            Debug.Assert(!string.IsNullOrEmpty(memberName));

            result = null;
            if (!_metaEnity.Fields.ContainsKey(memberName))
            {
                return false;
            }

            var metaField = _metaEnity.Fields[memberName];
            result = metaField.BrowseField(this._record);
            return true;
        }

    }
}
